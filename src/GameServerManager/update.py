from string import Template

class Update: 
    GameServerID = 882430
    
    UpdateString = "steamcmd +force_install_dir D:\Server001 +login anonymous +app_update $id +quit"
    
    def GetUpdateString(self):
        return Template(Update.UpdateString).substitute(id=Update.GameServerID)
    


up = Update()

print (up.GetUpdateString())