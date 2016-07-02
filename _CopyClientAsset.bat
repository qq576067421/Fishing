@echo off

set media_config_path=".\Media\Fishing\Config\"
set client_notpackasset_path=".\Client\Assets\NotPackAsset\"
set client_notpackasset_confg_path=".\Client\Assets\NotPackAsset\Media\Fishing\Config\"
set appdata_path=%AppData%\..\LocalLow\Cragon\Fishing\

xcopy /F /I /R /Y /K /E %media_config_path%Fishing.db %client_notpackasset_confg_path%Fishing.db

rd /S /Q %appdata_path%PC\NotPackAsset\
xcopy /F /I /R /Y /K /E %client_notpackasset_path%*.* %appdata_path%PC\NotPackAsset\

rd /S /Q %appdata_path%Android\NotPackAsset\
xcopy /F /I /R /Y /K /E %client_notpackasset_path%*.* %appdata_path%Android\NotPackAsset\

pause
echo on