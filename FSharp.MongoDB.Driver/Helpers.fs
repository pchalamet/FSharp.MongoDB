module FSharp.Helpers
open System
open MongoDB.Bson.Serialization

let fsharpType (typ : Type) =
    typ.GetCustomAttributes(typeof<CompilationMappingAttribute>, true) 
    |> Seq.cast<CompilationMappingAttribute>
    |> Seq.map(fun t -> t.SourceConstructFlags)
    |> Seq.tryHead

let createClassMapSerializer (type': Type) (classMap: BsonClassMap) =
    let concreteType = type'.MakeGenericType(classMap.ClassType)
    let ctor = concreteType.GetConstructor([| typeof<BsonClassMap> |])
    ctor.Invoke([| classMap |]) :?> IBsonSerializer
