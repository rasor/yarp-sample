projects:=Service1 Service2 Proxy
run: $(foreach project,$(projects),run.$(project))

$(foreach project,$(projects),run.$(project)) : run.% :
	powershell -NoProfile Start-Process dotnet -ArgumentList 'run', '--project', '$*/$*.csproj'
