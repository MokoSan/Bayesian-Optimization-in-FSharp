module Optimization.UnitTests

open NUnit.Framework
open Model 

[<SetUp>]
let Setup () =
    ()

[<Test>]
let ``Kernel Compute Should Return`` () =
    let expected = [1; 1; 1; 1]
    let actual   = [ 2; 1; 4; 1 ] |> List.map(fun d -> d / d)
    Assert.That(actual, Is.EqualTo(expected))
