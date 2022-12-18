module Model

open System
open System.Linq
open MathNet.Numerics.LinearAlgebra
open AcquisitionFunctions
open ObjectiveFunctions
open Surrogate
open Kernel
open Domain

[<Literal>]
let DEFAULT_EXPLORATION_PARAMETER : double = 0.01

let fitToModel (model : GaussianModel) (input : double) : unit =

    // If the data point has already been explored, don't spend cycles doing it again.
    if model.GaussianProcess.ObservedDataPoints.Any(fun d -> d.X = input) then ()
    else
        let result : double = 
            match model.ObjectiveFunction with
            | QueryContinuousFunction queryFunction               -> queryFunction input
            | QueryProcessByElapsedTimeInSeconds queryProcessInfo -> queryProcessByElapsedTimeInSeconds queryProcessInfo input
            | QueryProcessByTraceLog queryProcessInfo             -> queryProcessByTraceLog queryProcessInfo input

        let matchedKernel : double -> double -> double = getKernelFunction model

        let dataPoint : DataPoint = { X = input; Y = result } 
        model.GaussianProcess.ObservedDataPoints.Add dataPoint 

        let size                            : int = model.GaussianProcess.ObservedDataPoints.Count
        let mutable updatedCovarianceMatrix : Matrix<double> = Matrix<double>.Build.Dense(size, size)

        for rowIdx in 0..(model.GaussianProcess.CovarianceMatrix.RowCount - 1) do
            for columnIdx in 0..(model.GaussianProcess.CovarianceMatrix.ColumnCount - 1) do
                updatedCovarianceMatrix[rowIdx, columnIdx] <- model.GaussianProcess.CovarianceMatrix.[rowIdx, columnIdx]

        for iteratorIdx in 0..(size - 1) do
            let modelValueAtIndex : double = model.GaussianProcess.ObservedDataPoints.[iteratorIdx].X
            let value             : double = matchedKernel modelValueAtIndex dataPoint.X 
            updatedCovarianceMatrix[iteratorIdx, size - 1] <- value
            updatedCovarianceMatrix[size - 1, iteratorIdx] <- value

        updatedCovarianceMatrix[size - 1,  size - 1] <- matchedKernel dataPoint.X dataPoint.X
        model.GaussianProcess.CovarianceMatrix       <- updatedCovarianceMatrix

let createModelDiscrete (gaussianProcess   : GaussianProcess)
                        (objectiveFunction : ObjectiveFunction)
                        (min               : int)
                        (max               : int)
                        (resolution        : int) : GaussianModel =

    let inputs : double list = seq { for i in 0 .. resolution do i }
                               |> Seq.map(fun idx -> Math.Round( double( min + idx * (max - min) / ( resolution - 1))))
                               |> Seq.distinct
                               |> Seq.sort
                               |> Seq.toList
    { GaussianProcess = gaussianProcess; ObjectiveFunction = objectiveFunction; Inputs = inputs }

let createModel (gaussianProcess   : GaussianProcess) 
                (objectiveFunction : ObjectiveFunction) 
                (min               : double) 
                (max               : double)
                (resolution        : int) : GaussianModel = 

    let inputs : double list = seq { for i in 0 .. resolution do i }
                               |> Seq.map(fun idx -> min + double idx * (max - min) / (double resolution - 1.))
                               |> Seq.toList

    { GaussianProcess = gaussianProcess; ObjectiveFunction = objectiveFunction; Inputs = inputs }

let explore (model : GaussianModel) (goal : Goal) (iterations : int) : ModelResult =

    let applyFitToModel : (double -> unit) = fitToModel model
        
    // Add the first and last points to the model to kick things off.
    applyFitToModel model.Inputs.[0] 
    applyFitToModel (model.Inputs.Last())

    seq { 0..(iterations - 1) }
    |> Seq.iter(fun _ -> (

        // Select next point to sample via the surrogate function i.e. estimation of the objective that maximizes the acquisition function.
        let nextPoint : double = 
            let predictions                 : PredictionResult seq = predict model
            let acquisitionResults          : AcquisitionFunctionResult seq = predictions.Select(fun e -> (expectedImprovement model.GaussianProcess e goal DEFAULT_EXPLORATION_PARAMETER ))
            let optimumValueFromAcquisition : AcquisitionFunctionResult = acquisitionResults.MaxBy(fun e -> e.AcquisitionScore)
            optimumValueFromAcquisition.Input

        // Add the point to the model if it already hasn't been added.
        if model.GaussianProcess.ObservedDataPoints.Any(fun d -> d.X = nextPoint) then ()
        else
            applyFitToModel nextPoint
    ))

    let estimationResult : PredictionResult seq = predict model

    {
        ObservedDataPoints       = model.GaussianProcess.ObservedDataPoints
        AcquisitionResults = estimationResult.Select(fun e -> expectedImprovement model.GaussianProcess e goal DEFAULT_EXPLORATION_PARAMETER).ToList()
        PredictionResults         = estimationResult.ToList()
    }

let findOptima (model : GaussianModel) (goal : Goal) (iterations : int) : DataPoint = 
    let explorationResults : ModelResult = explore model goal iterations
    match goal with
    | Goal.Max -> explorationResults.ObservedDataPoints.MaxBy(fun o -> o.Y)
    | Goal.Min -> explorationResults.ObservedDataPoints.MinBy(fun o -> o.Y)
