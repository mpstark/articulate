// requires Windows 7, Windows 7 Service Pack 1, Windows Server 2003 Service Pack 2, Windows Server 2008, Windows Server 2008 R2, Windows Server 2008 R2 SP1, Windows Vista Service Pack 1, Windows XP Service Pack 3
// requires Windows Installer 3.1
// requires Internet Explorer 5.01
// WARNING: express setup (downloads and installs the components depending on your OS) if you want to deploy it on cd or network download the full bootsrapper on website below
// http://www.microsoft.com/downloads/en/details.aspx?FamilyID=5765d7a8-7722-4888-a970-ac39b33fd8ab

[CustomMessages]
dotnetfx45_title=.NET Framework 4.5

dotnetfx45_size=3 MB - 197 MB

;http://www.microsoft.com/globaldev/reference/lcid-all.mspx
en.dotnetfx45_lcid=''


[Code]
const
	dotnetfx45_url = 'http://download.microsoft.com/download/B/A/4/BA4A7E71-2906-4B2D-A0E1-80CF16844F5F/dotNetFx45_Full_setup.exe';

procedure dotnetfx45();
begin
	if (not netfxinstalled(NetFx45, '')) then
		AddProduct('dotNetFx45_Full_setup.exe',
			CustomMessage('dotnetfx45_lcid') + '/passive /norestart',
			CustomMessage('dotnetfx45_title'),
			CustomMessage('dotnetfx45_size'),
			dotnetfx45_url,
			false, false);
end;