module Optimization.Model

open System
open System.Linq
open System.Collections.Generic
open MathNet.Numerics.LinearAlgebra
open AcquisitionFunctions
open ObjectiveFunctions
open Kernel
open Domain

[<Literal>]
let DEFAULT_EXPLORATION_PARAMETER : double = 0.01

let createProcessWithSquaredExponentialKernel (squaredExponentialKernelParameters: SquaredExponentialKernelParameters) : GaussianProcess =
    { 
        KernelFunction     = SquaredExponentialKernel squaredExponentialKernelParameters 
        ObservedDataPoints = List<DataPoint>()
        CovarianceMatrix   = Matrix<double>.Build.Dense(1, 1)
    }

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

let predict (model: GaussianModel) : IEnumerable<PredictionResult> =
    let predictPoint (gaussianProcess : GaussianProcess) (input : double) : PredictionResult = 

        let matchedKernelFunction : (double -> double -> double) = getKernelFunction model

        let kStar : double[] =
            gaussianProcess.ObservedDataPoints
                           .Select(fun dp -> matchedKernelFunction input dp.X)
                           .ToArray()

        let yTrain : double[] =
            gaussianProcess.ObservedDataPoints
                           .Select(fun dp -> dp.Y)
                           .ToArray()

        let ks         : Vector<double> = Vector<double>.Build.Dense kStar
        let f          : Vector<double> = Vector<double>.Build.Dense yTrain

        // Common helper term. 
        let common     : Vector<double> = gaussianProcess.CovarianceMatrix.Inverse().Multiply ks
        // muStar = Kstar^T * K^-1 * f = common dot f
        let mu         : double         = common.DotProduct f
        let confidence : double         = Math.Abs( matchedKernelFunction input input  - common.DotProduct(ks) )

        { Mean = mu; LowerBound = mu - confidence; UpperBound = mu + confidence; Input = input }

    model.Inputs.Select(fun x -> predictPoint model.GaussianProcess x)

let createModelWithDiscreteInputs (gaussianProcess     : GaussianProcess)
                                  (objectiveFunction   : ObjectiveFunction)
                                  (acquisitionFunction : AcquisitionFunction)
                                  (min                 : int)
                                  (max                 : int)
                                  (resolution          : int) : GaussianModel =

    let inputs : double list = seq { for i in 0 .. resolution do i }
                               |> Seq.map(fun idx -> Math.Round( double(min + idx * (max - min) / (resolution - 1) )))
                               |> Seq.distinct
                               |> Seq.sort
                               |> Seq.toList
    { 
        GaussianProcess     = gaussianProcess 
        AcquisitionFunction = acquisitionFunction 
        ObjectiveFunction   = objectiveFunction
        Inputs              = inputs 
    }

let createModel (gaussianProcess     : GaussianProcess) 
                (objectiveFunction   : ObjectiveFunction) 
                (acquisitionFunction : AcquisitionFunction)
                (min                 : double) 
                (max                 : double)
                (resolution          : int) : GaussianModel = 

    let inputs : double list = seq { for i in 0 .. resolution do i }
                               |> Seq.map(fun idx -> min + double idx * (max - min) / (double resolution - 1.))
                               |> Seq.toList

    { 
        GaussianProcess     = gaussianProcess 
        AcquisitionFunction = acquisitionFunction 
        ObjectiveFunction   = objectiveFunction 
        Inputs              = inputs 
    }

let explore (model : GaussianModel) (goal : Goal) (iterations : int) : ExplorationResults  =

    let applyFitToModel : (double -> unit) = fitToModel model
        
    // Add the first and last points to the model to kick things off.
    applyFitToModel model.Inputs.[0] 
    applyFitToModel (model.Inputs.Last())

    let intermediateResults : List<IntermediateResult> = List<IntermediateResult>()

    let applyAcquisitionFunction (predictionResult : PredictionResult) : AcquisitionFunctionResult =
        match model.AcquisitionFunction with
        | ExpectedImprovement ->
            expectedImprovement { GaussianProcess = model.GaussianProcess; PredictionResult = predictionResult; Goal = goal }

    // Iterate with each step to find the most optimum next point.
    seq { 0..(iterations - 1) }
    |> Seq.iter(fun iteration -> (

        // Select next point to sample via the surrogate function i.e. estimation of the objective that maximizes the acquisition function.
        let predictions                 : PredictionResult seq = predict model
        let acquisitionResults          : AcquisitionFunctionResult seq = predictions.Select(fun predictionResult -> applyAcquisitionFunction predictionResult)
        let optimumValueFromAcquisition : AcquisitionFunctionResult = acquisitionResults.MaxBy(fun e -> e.AcquisitionScore)
        let nextPoint                   : double = optimumValueFromAcquisition.Input

        // Copy the state of the ObservedDataPoints for this iteration.
        let copyBuffer : DataPoint[] = Array.zeroCreate model.GaussianProcess.ObservedDataPoints.Count 
        model.GaussianProcess.ObservedDataPoints.CopyTo(copyBuffer) |> ignore

        let result : ModelResult = 
            {
                ObservedDataPoints = copyBuffer.ToList() 
                AcquisitionResults = acquisitionResults.ToList()
                PredictionResults  = predictions.ToList() 
            }

        intermediateResults.Add({ Result = result; NextPoint = nextPoint; Iteration = iteration })

        // Add the point to the model if it already hasn't been added.
        if model.GaussianProcess.ObservedDataPoints.Any(fun d -> d.X = nextPoint) then ()
        else applyFitToModel nextPoint
    ))

    let finalPredictions : PredictionResult seq = predict model

    let finalResult : ModelResult =
        {
            ObservedDataPoints = model.GaussianProcess.ObservedDataPoints
            AcquisitionResults = finalPredictions.Select(fun predictionResult -> applyAcquisitionFunction predictionResult).ToList()
            PredictionResults  = finalPredictions.ToList()
        }

    {
        IntermediateResults = intermediateResults
        FinalResult         = finalResult
    }

let findOptima (model : GaussianModel) (goal : Goal) (iterations : int) : OptimaResults = 
    let explorationResults : ExplorationResults = explore model goal iterations
    let optima = 
        match goal with
        | Goal.Max -> explorationResults.FinalResult.ObservedDataPoints.MaxBy(fun o -> o.Y)
        | Goal.Min -> explorationResults.FinalResult.ObservedDataPoints.MinBy(fun o -> o.Y)
    {
        ExplorationResults = explorationResults
        Optima             = optima
    }
