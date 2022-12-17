module Model

open System.Linq
open System.Collections.Generic
open MathNet.Numerics.LinearAlgebra
open AcquisitionFunctions
open Surrogate
open Kernel
open Domain

[<Literal>]
let DEFAULT_EXPLORATION_PARAMETER : double = 0.01

let addDataPointToModel (model : GaussianModel) (dataPoint : DataPoint) : unit =

    // TODO: Check if the model already contains the data point.
    model.GaussianProcess.DataPoints.Add dataPoint

    let size                            : int = model.GaussianProcess.DataPoints.Count
    let mutable updatedCovarianceMatrix : Matrix<double> = Matrix<double>.Build.Dense(size, size)

    for rowIdx in 0..(model.GaussianProcess.CovarianceMatrix.RowCount - 1) do
        for columnIdx in 0..(model.GaussianProcess.CovarianceMatrix.ColumnCount - 1) do
            updatedCovarianceMatrix[rowIdx, columnIdx] <- model.GaussianProcess.CovarianceMatrix.[rowIdx, columnIdx]

    for iteratorIdx in 0..(size - 1) do
        let modelValueAtIndex : double = model.GaussianProcess.DataPoints.[iteratorIdx].X
        let value             : double = squaredExponentialKernelCompute model.GaussianProcess.SquaredExponentialKernelParameters modelValueAtIndex dataPoint.X 
        updatedCovarianceMatrix[iteratorIdx, size - 1] <- value
        updatedCovarianceMatrix[size - 1, iteratorIdx] <- value

    updatedCovarianceMatrix[size - 1,  size - 1] <- squaredExponentialKernelCompute model.GaussianProcess.SquaredExponentialKernelParameters dataPoint.X dataPoint.X
    model.GaussianProcess.CovarianceMatrix       <- updatedCovarianceMatrix


let createModel (gaussianProcess  : GaussianProcess) 
                (query            : double -> double) 
                (min              : double) 
                (max              : double)
                (resolution       : int) : GaussianModel = 

    // Random Uniform Initialization of Inputs.
    let inputs : List<double> = (seq { for i in 0 .. resolution do i }
                                |> Seq.map(fun idx -> min + double idx * (max - min) / (double resolution - 1.))).ToList()
    { GaussianProcess = gaussianProcess; Query = query; Inputs = inputs }

let findOptima (model : GaussianModel) (goal : Goal) (iterations : int) : ModelResult =

    // Add the extreme points.
    addDataPointToModel model { X = model.Inputs.[0]; Y = model.Query model.Inputs.[0] }
    addDataPointToModel model { X = model.Inputs.Last(); Y = model.Query( model.Inputs.Last() )}

    for iterationIdx in 0..(iterations - 1) do

        // Select next point to sample.
        let nextPointToSample : double = 
            let surrogateEstimations        : List<EstimationResult> = estimateAtRange model
            let maxAcquisition              : List<AcquisitionFunctionResult> = surrogateEstimations.Select(fun e -> (expectedImprovement model.GaussianProcess e goal DEFAULT_EXPLORATION_PARAMETER )).ToList() 
            let optimumValueFromAcquisition : AcquisitionFunctionResult = maxAcquisition.MaxBy(fun e -> e.Y)
            optimumValueFromAcquisition.X

        // Add the point to the model.
        if model.GaussianProcess.DataPoints.Any(fun d -> d.X = nextPointToSample) then ()
        else
            addDataPointToModel model { X = nextPointToSample; Y = model.Query ( nextPointToSample )}

    let estimationResult : List<EstimationResult> = estimateAtRange model

    {
        Input                    = model.GaussianProcess.DataPoints
        AcquistionFunctionResult = estimationResult.Select(fun e -> expectedImprovement model.GaussianProcess e goal DEFAULT_EXPLORATION_PARAMETER).ToList() 
        EstimationResult         = estimationResult
    }
