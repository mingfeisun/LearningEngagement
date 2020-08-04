### Installation for Ubuntu
1. Download Unity3D at [here](https://beta.unity3d.com/download/dad990bf2728/public_download.html) and install.

2. Add Unity3D to bin command:
``` bash
sudo mv Unity-2018.2.7f1 /opt/Unity3D
sudo ln -s /opt/Unity3D/Editor/Unity /usr/bin/unity3d
```

3. Install .Net core SDK
``` bash 
wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-3.1
```