module Optimization.Charting

open System
open System.Text.RegularExpressions
open System.IO
open System.Linq
open Plotly.NET
open Plotly.NET.ImageExport
open ImageMagick

let chartResult (result : ModelResult) (nextPoint : Nullable<double>) (title : string) : GenericChart.GenericChart  =  

    let observedDataPoints : GenericChart.GenericChart =
        let ordered : DataPoint seq = result.ObservedDataPoints.OrderBy(fun o -> o.X)
        Chart.Line(
            ordered.Select(fun o -> o.X),
            ordered.Select(fun o -> o.Y),
            Name = "Observed Data",
            ShowMarkers=true
        )
        |> Chart.withAxisAnchor(Y = 1)

    let predictedMean : GenericChart.GenericChart = 
        let ordered : PredictionResult seq = result.PredictionResults.OrderBy(fun a -> a.Input)

        Chart.Range( 
            xy    = ordered.Select(fun a -> (a.Input, a.Mean)),
            upper = ordered.Select(fun a -> a.UpperBound),
            lower = ordered.Select(fun a -> a.LowerBound),
            mode  = StyleParam.Mode.Lines_Markers,
            RangeColor = Color.fromString "lightblue",
            Name  = "Predictions",
            MarkerSymbol = StyleParam.MarkerSymbol.Square,
            ShowMarkers = true
        )
        |> Chart.withLineStyle(Dash = StyleParam.DrawingStyle.Dot, Color = Color.fromString "green")

    let nextPointScatter : GenericChart.GenericChart = 
        match nextPoint.HasValue with
        | true -> 
            Chart.Line( 
                [nextPoint.Value; nextPoint.Value], 
                [Math.Min(result.ObservedDataPoints.Min(fun o -> o.Y), result.PredictionResults.Min(fun o -> o.Mean)); 
                    Math.Max(result.ObservedDataPoints.Max(fun o -> o.Y), result.PredictionResults.Max(fun o -> o.Mean))],
                Name = "Next Point"
            )
            |> Chart.withLineStyle(Width = 3., Color = Color.fromString "red")

        // For the last iteration, we don't draw the next point.
        | false ->
            Chart.Line(
                [],
                [],
                Name = "Next Point"
            )

    let nonAcquisitionChart : GenericChart.GenericChart =
        [observedDataPoints; predictedMean; nextPointScatter]
        |> Chart.combine
        |> Chart.withTitle title

    let acquisitionResultsScatter : GenericChart.GenericChart = 
        let ordered : AcquisitionFunctionResult seq = result.AcquisitionResults.OrderBy(fun a -> a.Input)
        Chart.Line(
            ordered.Select(fun a -> a.Input),
            ordered.Select(fun a -> a.AcquisitionScore),
            Name = "Acquisition Results"
        )

    let acquisitionChart : GenericChart.GenericChart = 
        [acquisitionResultsScatter]
        |> Chart.combine

    let grid : GenericChart.GenericChart =
        [ nonAcquisitionChart; acquisitionChart ]
        |> Chart.SingleStack(Pattern = StyleParam.LayoutGridPattern.Coupled)

    grid


let chartAllResults (results : OptimaResult) : GenericChart.GenericChart seq =
    let intermediateCharts : GenericChart.GenericChart seq = 
        results.ExplorationResults.IntermediateResults
        |> Seq.map(fun i -> (
            chartResult i.Result (Nullable(i.NextPoint)) $"Iteration: {i.Iteration}. Next Point: {Math.Round(i.NextPoint, 2)}"
        ))

    // Final chart doesn't have a next point.
    let finalChart : GenericChart.GenericChart = 
        chartResult results.ExplorationResults.FinalResult (Nullable(results.Optima.X)) $"Final Iteration - Optima at {Math.Round(results.Optima.X,2)}"

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

let saveGif (baseOutputPath: string) (gifSavePath : string) : unit =  
    let allImages : string seq = Directory.GetFiles(baseOutputPath, "*.png") 
                                 |> Seq.sortBy(fun d -> int (Regex.Replace(Path.GetFileName(d), "[^0-9]", "")))
    use images : MagickImageCollection = new MagickImageCollection()
    allImages
    |> Seq.iter(fun path -> (
        let image : MagickImage = new MagickImage(path)
        images.Add image
        images.[images.Count - 1].AnimationDelay <- 100
    ))

    // Delay the last iteration to underscore the optima.
    images.[images.Count - 1].AnimationDelay <- 500

    images.Optimize() |> ignore
    images.Write(gifSavePath)
