#!/bin/bash
git pull origin master
git submodule update --init --recursive
rm -rf ./bin/live_release_old
mv ./bin/live_release ./bin/live_release_old
dotnet build SwarmHelpBot.sln --configuration Release -o ./bin/live_release
screen -dmS SwarmHelpBot dotnet bin/live_release/SwarmHelpBot.dll $1
