module ObjectiveFunctions

open System.Diagnostics

let queryProcessByElapsedTimeInSeconds (queryProcessInfo : QueryProcessInfo) (input : double) : double =

    let stopWatch = Stopwatch()
    stopWatch.Start()

    use unstartedProcess = new Process()
    unstartedProcess.StartInfo.FileName        <- queryProcessInfo.WorkloadPath 
    unstartedProcess.StartInfo.UseShellExecute <- false
    unstartedProcess.StartInfo.Arguments       <- queryProcessInfo.ApplyArguments ( input.ToString() )

    unstartedProcess.Start()       |> ignore
    unstartedProcess.WaitForExit() |> ignore

    stopWatch.Stop()

    let result : double = double stopWatch.ElapsedMilliseconds / 1000.

    printfn "%A -> %A" input result
    result
