###Do package autoupdate thing!!!
like this `find . -maxdepth 1 -name "*" | sed -e "s|./||g" | xargs -n 1 dotnet add ../bot-core package`