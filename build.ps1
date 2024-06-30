dotnet build

echo "Copying..."

cp texture_head.png bin\Debug\net8.0
cp texture_lefthand.png bin\Debug\net8.0
cp texture_righthand.png bin\Debug\net8.0

cp openvr_api.dll bin\Debug\net8.0
cp README.md bin\Debug\net8.0

echo "Done"