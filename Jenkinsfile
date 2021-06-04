pipeline {
	agent any
	
	environment {
		BuildConfiguration="Debug"
	}

	stages {
		stage('Build') {
			steps {
				sh "dotnet build -c $BuildConfiguration"
			}
		}

		stage('Test') {
			steps {
				sh 'dotnet test -c $BuildConfiguration --no-build'
			}
		}

		
		stage('Pack') {
			steps {
				sh 'dotnet pack TGS.Core.Infrastructures.Nats/TGS.Core.Infrastructures.Nats.csproj -c $BuildConfiguration --no-build --no-restore'
			}
		}
		
		stage('Push Nuget') {   
			steps {
				sh  'dotnet nuget push TGS.Core.Infrastructures.Nats/bin/$BuildConfiguration/*.nupkg --source "raypro" --skip-duplicate'
			}
		}
	}
}