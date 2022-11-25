// Based on https://safe-stack.github.io/docs/recipes/build/add-build-script/

open Fake.Core
open Fake.IO
open System

module Helpers =
  let redirect createProcess =
    createProcess
    |> CreateProcess.redirectOutputIfNotRedirected
    |> CreateProcess.withOutputEvents Console.WriteLine Console.WriteLine

  let createProcess exe arg dir =
    CreateProcess.fromRawCommandLine exe arg
    |> CreateProcess.withWorkingDirectory dir
    |> CreateProcess.ensureExitCode

  let dotnet = createProcess "dotnet"

  let run proc arg dir =
    proc arg dir
    |> Proc.run
    |> ignore

  let runPararell p =
    p |> Seq.toArray
    |> Array.map redirect
    |> Array.Parallel.map Proc.run
    |> ignore


module Paths =
  let client = Path.getFullName "src/Client"
  let output = Path.getFullName "src/Client/output"
  let server = Path.getFullName "src/Server"
  let shared = Path.getFullName "src/Shared"

  let clientTest = Path.getFullName "test/Client"
  let clientTestOutput = Path.getFullName "test/Client/output"
  let serverTest = Path.getFullName "test/Server"


open Helpers

let execContext = Context.FakeExecutionContext.Create false "build.fsx" [ ]
Context.setExecutionContext (Context.RuntimeContext.Fake execContext)

Target.create "clean" (fun _ ->
    run dotnet $"fable clean --yes -o dist" ".")

Target.create "run" (fun _ ->
    run dotnet "build" Paths.shared
    [ dotnet "watch run" Paths.server
      dotnet $"fable watch {Paths.client} -o dist" "."
      dotnet $"perla serve" "." ]
    |> runPararell)

Target.create "test" (fun _ ->
  run dotnet $"fable clean --yes -o {Paths.clientTestOutput}" "."
  [ dotnet "watch run" Paths.serverTest
    dotnet $"fable watch {Paths.clientTest} -o {Paths.clientTestOutput}" "." 
    dotnet $"perla serve -p 8081" "." ]
  |> runPararell)

open Fake.Core.TargetOperators

let dependencies = [
  "clean"
    ==> "run"
]

[<EntryPoint>]
let main args =
  try
    match args with
    | [| target |] -> Target.runOrDefault target
    | _ -> Target.runOrDefault "run"
    0
  with e ->
    printfn "%A" e
    1
