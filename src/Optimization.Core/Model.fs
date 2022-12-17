module Model

open System.Linq
open System.Collections.Generic
open MathNet.Numerics.LinearAlgebra
open AcquisitionFunctions
open ObjectiveFunctions
open Surrogate
open Kernel
open Domain

[<Literal>]
let DEFAULT_EXPLORATION_PARAMETER : double = 0.01

let fitToModel (model : GaussianModel) (input : double) : unit =

    // TODO: Check if the model already contains the data point.

    // Get the result from the objective function.
    let result : double = 
        match model.ObjectiveFunction with
        | QueryContinuousFunction queryFunction               -> queryFunction input
        | QueryProcessByElapsedTimeInSeconds queryProcessInfo -> queryProcessByElapsedTimeInSeconds queryProcessInfo input

    let dataPoint : DataPoint = { X = input; Y = result } 
    model.GaussianProcess.ObservedDataPoints.Add dataPoint 

    let size                            : int = model.GaussianProcess.ObservedDataPoints.Count
    let mutable updatedCovarianceMatrix : Matrix<double> = Matrix<double>.Build.Dense(size, size)

    for rowIdx in 0..(model.GaussianProcess.CovarianceMatrix.RowCount - 1) do
        for columnIdx in 0..(model.GaussianProcess.CovarianceMatrix.ColumnCount - 1) do
            updatedCovarianceMatrix[rowIdx, columnIdx] <- model.GaussianProcess.CovarianceMatrix.[rowIdx, columnIdx]

    for iteratorIdx in 0..(size - 1) do
        let modelValueAtIndex : double = model.GaussianProcess.ObservedDataPoints.[iteratorIdx].X
        let value             : double = squaredExponentialKernelCompute model.GaussianProcess.SquaredExponentialKernelParameters modelValueAtIndex dataPoint.X 
        updatedCovarianceMatrix[iteratorIdx, size - 1] <- value
        updatedCovarianceMatrix[size - 1, iteratorIdx] <- value

    updatedCovarianceMatrix[size - 1,  size - 1] <- squaredExponentialKernelCompute model.GaussianProcess.SquaredExponentialKernelParameters dataPoint.X dataPoint.X
    model.GaussianProcess.CovarianceMatrix       <- updatedCovarianceMatrix

let createModel (gaussianProcess   : GaussianProcess) 
                (objectiveFunction : ObjectiveFunction) 
                (min               : double) 
                (max               : double)
                (resolution        : int) : GaussianModel = 

    // Random Uniform Initialization of Inputs. This implies that we assigning a uniform distribution for our priors.
    let inputs : double list = seq { for i in 0 .. resolution do i }
                               |> Seq.map(fun idx -> min + double idx * (max - min) / (double resolution - 1.))
                               |> Seq.toList

    { GaussianProcess = gaussianProcess; ObjectiveFunction = objectiveFunction; Inputs = inputs }

let findOptima (model : GaussianModel) (goal : Goal) (iterations : int) : ModelResult =

    // Add the first and last points to the model to kick things off.
    fitToModel model model.Inputs.[0]
    fitToModel model (model.Inputs.Last())

    for _ in 0..(iterations - 1) do

        // Select next point to sample via the surrogate function i.e. estimation of the objective that maximizes the acquisition function.
        let nextPointToSample : double = 
            let surrogateEstimations        : List<EstimationResult> = estimateAtRange model
            let maxAcquisition              : List<AcquisitionFunctionResult> = surrogateEstimations.Select(fun e -> (expectedImprovement model.GaussianProcess e goal DEFAULT_EXPLORATION_PARAMETER )).ToList() 
            let optimumValueFromAcquisition : AcquisitionFunctionResult = maxAcquisition.MaxBy(fun e -> e.Y)
            optimumValueFromAcquisition.X

        // Add the point to the model if it already hasn't been added.
        if model.GaussianProcess.ObservedDataPoints.Any(fun d -> d.X = nextPointToSample) then ()
        else
            fitToModel model nextPointToSample

    let estimationResult : List<EstimationResult> = estimateAtRange model

    {
        Input                    = model.GaussianProcess.ObservedDataPoints
        AcquistionFunctionResult = estimationResult.Select(fun e -> expectedImprovement model.GaussianProcess e goal DEFAULT_EXPLORATION_PARAMETER).ToList() 
        EstimationResult         = estimationResult
    }
