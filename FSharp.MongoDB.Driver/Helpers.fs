module FSharp.Helpers
open System

let fsharpType (typ : Type) =
    typ.GetCustomAttributes(typeof<CompilationMappingAttribute>, true) 
    |> Seq.cast<CompilationMappingAttribute>
    |> Seq.map(fun t -> t.SourceConstructFlags)
    |> Seq.tryHead
