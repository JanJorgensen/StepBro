all: build

build: $STEPBRO_DEBUG_ADAPTER_RELEASE

debug: $STEPBRO_DEBUG_ADAPTER_DEBUG

$STEPBRO_DEBUG_ADAPTER_RELEASE:
	msbuild /v:minimal /restore /p:Configuration=Release src/csharp/stepbro-debug-adapter.csproj

$STEPBRO_DEBUG_ADAPTER_DEBUG:
	msbuild /v:minimal /restore /p:Configuration=Debug src/csharp/stepbro-debug-adapter.csproj