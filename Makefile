config ?= Debug
version ?= 0.0.0


build:
	dotnet build -c $(config)

test:
	dotnet test -c $(config)

nuget:
	dotnet pack -c $(config) -p:Version=$(version) -o .out
