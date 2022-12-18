module ObjectiveFunctions

open System.IO
open Microsoft.Diagnostics.Tracing
open System.Diagnostics

open TraceCollector

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
    result

// TODO: Discretize.
let queryProcessByTraceLog (queryProcessInfoByTraceLog : QueryProcessInfoByTraceLog) (input : double) : double =

    // Start the trace collection process with the appropriate parameters.

    use unstartedProcess = new Process()
    unstartedProcess.StartInfo.FileName        <- queryProcessInfoByTraceLog.WorkloadPath 
    unstartedProcess.StartInfo.UseShellExecute <- false
    unstartedProcess.StartInfo.Arguments       <- queryProcessInfoByTraceLog.ApplyArguments ( input.ToString() )

    let environmentVariables : Map<string, string> = queryProcessInfoByTraceLog.EnvironmentVariables(int input)
    environmentVariables
    |> Map.iter(fun k v -> unstartedProcess.StartInfo.EnvironmentVariables.[k] <- v )
    |> ignore

    if System.IO.Directory.Exists queryProcessInfoByTraceLog.OutputPath then ()
    else 
        System.IO.Directory.CreateDirectory(queryProcessInfoByTraceLog.OutputPath) |> ignore
        ()

    let etlPath = Path.Combine(queryProcessInfoByTraceLog.OutputPath, $"{input}.etl")
    let (startTraceCommand, startProcess) : string * Process = startTrace queryProcessInfoByTraceLog.TraceParameters etlPath 

    unstartedProcess.Start()       |> ignore
    unstartedProcess.WaitForExit() |> ignore

    stopTrace startTraceCommand  |> ignore
    startProcess.Dispose()       |> ignore

    let mutable tracePathFinal = etlPath + ".zip"

    if Path.GetExtension(tracePathFinal) = ".zip" then 
        let zippedReader = ZippedETLReader(tracePathFinal);
        zippedReader.UnpackArchive();
        tracePathFinal <- zippedReader.EtlFileName;
        ()
    else ()

    let traceLog = Microsoft.Diagnostics.Tracing.Etlx.TraceLog.OpenOrConvert(tracePathFinal);

    let result : double = queryProcessInfoByTraceLog.TraceLogApplication traceLog 
    result
