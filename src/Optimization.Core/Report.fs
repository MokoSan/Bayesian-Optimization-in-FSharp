module Report

open System.Text
open Model
open XPlot.Plotly
open Charting 
open Giraffe.ViewEngine

let generateReport (outputPath : string) (optimaResults : OptimaResults)  : unit =
    ()

(*
    let plots : PlotlyChart seq =
        optimaResults.ExplorationResults.IntermediateResults
        |> Seq.mapi(fun idx result -> plotResult result $"Iteration: {idx}" )
    let finalPlot : PlotlyChart =
        plotResult optimaResults.ExplorationResults.FinalResult "Final Iteration"
    let optima : DataPoint      =  optimaResults.Optima

    let sb : StringBuilder = StringBuilder() 

    let head : string = 
        let headNode = 
            head [] [ 
                title [] [ str "Optimization Results" ]
            ]
        RenderView.AsString.htmlDocument headNode 
*)
