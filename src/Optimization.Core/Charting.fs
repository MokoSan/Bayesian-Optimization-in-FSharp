module Charting

open PuppeteerSharp
open System.Linq
open System.IO
open XPlot.Plotly

let plotExploration (result : ModelResult) (title : string) : PlotlyChart =  
    let observedDataPoints : Scatter =
        let ordered : DataPoint seq = result.ObservedDataPoints.OrderBy(fun o -> o.X)
        Scatter(
            x    = ordered.Select(fun o -> o.X),
            y    = ordered.Select(fun o -> o.Y),
            name = "Observed Data",
            mode = "lines+markers"
        )

    let predictedMean : Scatter = 
        let ordered : PredictionResult seq = result.PredictionResults.OrderBy(fun a -> a.Input)
        Scatter(
            x    = ordered.Select(fun a -> a.Input),
            y    = ordered.Select(fun a -> a.Mean),
            name = "Predictions",
            mode = "lines+markers"
        )

    let acquisitionResultsScatter : Scatter = 
        let ordered : AcquisitionFunctionResult seq = result.AcquisitionResults.OrderBy(fun a -> a.Input)
        Scatter(
            x    = ordered.Select(fun a -> a.Input),
            y    = ordered.Select(fun a -> a.AcquisitionScore),
            name = "Acquisition Results"
        )

    // TODO: Add next point.

    let layout : Layout.Layout = Layout.Layout()
    layout.title <- title 

    ([observedDataPoints; acquisitionResultsScatter; predictedMean ], layout)
    |> Chart.Plot
    |> Chart.WithWidth 700
    |> Chart.WithHeight 500

let plotAndSave (result : ModelResult) (outputPath : string) (title : string) : unit = 
    let plot : PlotlyChart = plotExploration result title 
    let tempHtmlFile : string = $"{outputPath}_file.html"

    async {

        // Write Plot.
        File.WriteAllTextAsync(tempHtmlFile, plot.GetHtml()) |> Async.AwaitTask |> ignore

        use browserFetcher : BrowserFetcher = new BrowserFetcher()
        browserFetcher.DownloadAsync() |> Async.AwaitTask |> ignore

        let launchOptions : LaunchOptions = LaunchOptions()
        launchOptions.Headless <- true

        // TODO: FIX....
        //let! browser = Puppeteer.LaunchAsync(launchOptions) |> Async.AwaitTask
        //let! page    = browser.NewPageAsync()               |> Async.AwaitTask
        //page.GoToAsync(tempHtmlFile)                        |> Async.AwaitTask |> ignore
        //page.ScreenshotAsync outputPath                     |> Async.AwaitTask |> ignore
    }|> Async.RunSynchronously
