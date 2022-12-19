module Charting

open System.IO
open System.Linq
open System
open Plotly.NET
open Plotly.NET.ImageExport

let chartResult (result : ModelResult) (nextPoint : Nullable<double>) (title : string) : GenericChart.GenericChart  =  
    let observedDataPoints : GenericChart.GenericChart =
        let ordered : DataPoint seq = result.ObservedDataPoints.OrderBy(fun o -> o.X)
        Chart.Line(
            ordered.Select(fun o -> o.X),
            ordered.Select(fun o -> o.Y),
            Name = "Observed Data",
            ShowMarkers=true
        )

    let predictedMean : GenericChart.GenericChart = 
        let ordered : PredictionResult seq = result.PredictionResults.OrderBy(fun a -> a.Input)
        Chart.Line(
            ordered.Select(fun a -> a.Input),
            ordered.Select(fun a -> a.Mean),
            Name = "Predictions",
            ShowMarkers = true
        )

    let acquisitionResultsScatter : GenericChart.GenericChart = 
        let ordered : AcquisitionFunctionResult seq = result.AcquisitionResults.OrderBy(fun a -> a.Input)
        Chart.Line(
            ordered.Select(fun a -> a.Input),
            ordered.Select(fun a -> a.AcquisitionScore),
            Name = "Acquisition Results"
        )

    let nextPointScatter : GenericChart.GenericChart = 
        match nextPoint.HasValue with
        | true -> 
            Chart.Line( 
                [nextPoint.Value; nextPoint.Value], 
                [result.ObservedDataPoints.Min(fun o -> o.Y); result.ObservedDataPoints.Max(fun o -> o.Y)],
                Name = "Next Point"
            )
        | false ->
            Chart.Line(
                [],
                [],
                Name = "Next Point"
            )

    [observedDataPoints; predictedMean; acquisitionResultsScatter; nextPointScatter]
    |> Chart.combine
    |> Chart.withTitle title

let chartAllResults (results : ExplorationResults) : GenericChart.GenericChart seq =
    let intermediateCharts : GenericChart.GenericChart seq = 
        results.IntermediateResults
        |> Seq.map(fun i -> (
            chartResult i.Result (Nullable(i.NextPoint)) $"Iteration: {i.Iteration}"
        ))

    // Final chart doesn't have a next point.
    let finalChart : GenericChart.GenericChart = 
        chartResult results.FinalResult (Nullable()) "Final Iteration"

    [finalChart]
    |> Seq.append intermediateCharts

let chartOptima (results : OptimaResults) : GenericChart.GenericChart seq =
    // Chart Intermediate Results
    let intermediateCharts : GenericChart.GenericChart seq = 
        results.ExplorationResults.IntermediateResults
        |> Seq.map(fun i -> (
            chartResult i.Result (Nullable(i.NextPoint)) $"Iteration: {i.Iteration}"
        ))

    let finalChart : GenericChart.GenericChart = 
        chartResult results.ExplorationResults.FinalResult (Nullable(results.Optima.X)) "Final Iteration"

    [finalChart]
    |> Seq.append intermediateCharts

let saveChart (outputPath : string) (chart : GenericChart.GenericChart) : unit =
    chart
    |> Chart.savePNG(outputPath)

let saveCharts (baseOutputPath : string) (charts : GenericChart.GenericChart seq) : unit =

    if (Directory.Exists(baseOutputPath)) then ()
    else Directory.CreateDirectory(baseOutputPath)  |> ignore

    charts
    |> Seq.iteri(fun idx chart -> saveChart (Path.Combine(baseOutputPath, $"{idx}")) chart)
