config ?= Debug
version ?= 0.0.0


build:
	dotnet build

test:
	dotnet test

nuget:
	dotnet pack -c $(config) -p:Version=$(version) -o .out
