## do not disturb us with helps. or helping you make client and website. were not gonna help you since it disturbs us alot during break/work thank you

<div align="center">
    <p>
      <h1>BubbaBlox | fixed fr</h1>
    </p>
</div>

its not recommended to setup bubbablox without a VPS, Domain, game server (opinonal). if you meet those requirement then yes you can run a revival

(original guide by <a href="https://github.com/SrCookie450">SrCookie450</a>, changed and site fixed by <a href="https://github.com/harryzawg">harryzawg</a>, tutorial edited by <a href="https://github.com/unknownluau">unknownluau</a> and <a href="https://github.com/potatoluau">potatoluau</a>)

## things you need

- <a href="https://nodejs.org/dist/v18.16.1/node-v18.16.1-x64.msi">Node.js</a>, *to run the renderer/build panel*
- <a href="https://sbp.enterprisedb.com/getfile.jsp?fileid=1258627">PostgreSQL</a>, *for the database*
- <a href="https://builds.dotnet.microsoft.com/dotnet/Sdk/6.0.412/dotnet-sdk-6.0.412-win-x64.exe">.NET 6.0</a>, *to run the website*
- <a href="https://go.dev/dl/go1.20.6.windows-amd64.msi">Go</a>, *for asset validation*
- <a href="https://www.python.org/ftp/python/3.12.8/python-3.12.8-amd64.exe">Python (3.12)</a>, *for image validation, make sure to add to path in setup!*
- open cmd and paste this in once u finish installing python (REQUIRED) ``pip install fastapi aiohttp pydub uvicorn python-magic python-magic-bin==0.4.14 python-multipart cryptography``
----------------------------
- <a href="https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-full.7z">FFmpeg</a> {IF YOU WANT AUDIO'S TO WORK}
- <a href="https://code.visualstudio.com/Download">(optional) VSCode</a> {RECOMMENDED}
- <a href="https://mh-nexus.de/downloads/HxDPortableSetup.zip">(required for patching) HxD</a> {USED FOR PATCHING RCC}

## hosting suggestions
- we suggest datalix for vps and spaceship for domain
--------
## cloudflare setup

- go to zero trust cloudflare
- 1 ofc first add ur domain in there
- 2 then go to quick search and type tunnels
- 3 go to Networks > Connectors then create a new tunnel
- 4 select cloudflared then name it anything it doesnt matter
- 5 follow the tutorial in there it has the tutorial on how to connect a tunnel
- 6 go to the tunnel u just made then go to published application routes
- 7 then press add a published application route after that in domain select your domain dont add anything in subdomain, leave path empty, and in service make it http and url localhost press save
---
- go to DNS > Records then delete everything but keep the NS records
- then add a record with Type ``A`` Name ``games`` Content ``your.vps.ip`` Proxy status ``off`` TTL ``Auto``
- after that add a record with Type ``CNAME`` Name ``www`` Content ``your.domain`` Proxy status ``on`` TTL ``Auto``

keep in mind if you see "bad gateway" once finishing. it means cloudflare is getting connected to your website. start runall.bat to see website online
---------
## hosting clients on localhost
- to host a client on localhost you have to go to ```C:\Windows\System32\drivers\etc``` and open hosts file in notepad++ or notepad all work, then you have to scroll to the very end be careful so u dont fuck up some stuff, make the domain 10 letters any domain works just make sure its not a regular domain u use it will probably fuck some shit up.
- Example: ``127.0.0.1 revival.io``

potatoluau : who hosts client on localhost son get a domain

  
## requirements
- windows server. 2022-2025, all though linux wasn't actually tested but it is recommended to use VM if your using linux since they wont work properly due to EXE not being supported in linux lol.
---
- a 10 character long domain that supports both HTTP and HTTPS
-- if you wanna make a moblie client. WWW. or ANY. is required for that.
- you will need some knowledge of how things like this work (you should have at least some experience with websites and coding to be able to host this. it's really not hard to set up if you know what you're doing.)

## vulnerabilitys (RECOMMENDED)

- go to ```your_revival_path/Roblox/Roblox.Website/Controllers/Internal/Other/AggHunt.cs``` and change the api key there, people can do https://your.domain/game/EggHunt.ashx?apiKey=TopSecretAggHuntKeyDoNOTLEAKPlz&placeId=1&playerId=REPLACE_WITH_USER_ID&eggId=REPLACE_WITH_ASSET_ID and give anything to any user, the place id doesnt matter.
- go to ```your_revival_path/Roblox/Roblox.Configuration/Class1.cs``` and change the UserAgentBypassSecret, change it to anything random it doesnt matter 

## database 

- open Command Prompt, and use CD to go into your PostgreSQL folder. it should be at ```C:\Program Files\PostgreSQL\(your postgres version, if you followed the guide it will be 13)\bin```
- copy the schema.sql file in ```api/sql``` to that PostgreSQL bin folder, then run in a Command Prompt window in that folder:

```psql --username=** --dbname=* < schema.sql``` replace --username with your PG user, db with your database name.

- ```*``` = the name of the database you want to use, if this is your first time installing, use postgres
- ```**``` = your postgres username, default is postgres if you didn't set any in the setup

## setting up backend

- rename the ```appsettings.example.json``` file in ```Roblox/Roblox.Website``` to just ```appsettings.json```, then open it.
- in the appsettings file, change the default POSTGRES line that looks like this:
- ```"Postgres": "Host=127.0.0.1; Database=bubbabloxnew; Password=test; Username=postgres; Maximum Pool Size=20",```
- in the ```appsettings.json``` make sure the GSIPAddress is either your internet ip (PLS NEVER HOST THIS ON UR PC), you will have to use the IP with your vps's one.
- change the sessions key: ```"Jwt": { "Sessions": "YourVerySecureSessionsKeyMakeThisReallyLong" },``` to ANYTHING make it REALLY long bc ppl can crack passwords but it would probably take years just pls change it to make it long

to:

``` "Postgres": "Host=127.0.0.1; Database=*; Password=your Postgres password; Username=**; Maximum Pool Size=20",```

- ```*``` = the name of the database you want to use, if this is your first time installing, use postgres
- ```**``` = your postgres username, default is postgres if you didn't set any in the setup

- press ```CTRL + H``` and change ```C:\\Users\\Admin\\Desktop\\Revival\\ecsr\\ecsrev-main\\services\\``` to ```C:\\whereever your BubbaBlox folder is\\``` (make sure it's double slashed! so it should look like ```C:\\folder1\\folder2\\```)
- you should update everything in the appsettings.json file to your configuration.
- you should also rename ```game-servers.example.json``` in the ```Roblox/Roblox.Website``` folder to just ```game-servers.json```
- go to the ```renderer``` folder, rename the file named ```config.example.json``` to ```config.json``` and change everything inside of it so it works with your main site and matches your appsettings.json.
you should change GameServerAuthorization and the Authorization under Render in your appsettings.json to the Authorization in your renderer config.json.

## setting up thumbnails and frontend

- first off, go into ```Roblox/Roblox.Website/Middleware/CorsMiddleware.cs```, and replace everything that has ```bbblox.org``` in it with your domain, this is so thumbnails can load.
- go into the ```api``` folder, 
- create a folder named ```storage``` inside of that api folder,
- inside the ```storage``` folder you just made, make a folder named ```asset```,
- then go back to the ```api``` folder, and go into ```public/images```. then, make a folder named ```thumbnails``` and ```groups```.
- open Command Prompt and use CD to go into the ```admin``` folder, then run ```npm i``` and ```npm run build```
- go to ```2016-roblox-main``` and rename the file named config.example.json to config.json.
- replace ```your.domain``` with your actual domain inside of that config.json file.
- in a Command Prompt window, and use CD to go into that same folder (```2016-roblox-main```), and run ```npm i``` and then ```npm run build```
- also in a Command Prompt window, and use CD to go into ```renderer```, and run ```npm i``` and then run ```npm run build```

## discord

- go to the <a href="https://discord.com/developers/applications">Discord Developer Portal</a> and make a new application.
- go into OAuth2, and replace the client id in the appsettings.json with your new client ID and client secret.
- add your redirect URL under the client ID section to be ```https://your.domain/discordcb```, replacing your.domain with your domain. do the same for ```https://your.domain/forgotcb```, and ```https://your.domain/logincb```.
- update the client ID, secret and add your new redirect URLs that you just added in the portal to ```appsettings.json``` or else it won't work.

# RCC domain patch

- download [HxD](https://mh-nexus.de/en/downloads.php?product=HxD20), go into the RCCService folder, RCCService.exe file into it. make sure the domain you are using for this is exactly 10 characters. it needs to be EXACTLY 10 characters.
- the reason for this is the way that RCC was compiled, it was set to use Roblox's domain which is 10 characters. just replace it with your 10 char domain (CTRL + R, then do bbblox.org as the string then replace it with your domain. make sure your direction is all)
- do the same for the client.
- also, change the domain in AppSettings.xml to your domain. (for clients and RCC)
- **if you decide to use your own RCC/client, you will have to add the hash of the client in ClientData.cs under ```Roblox/Roblox.Website/Controllers``` and also search for NS1 and replace your domain with erablx.lol for NS1, NS2 and NS3. [example](https://zawg.ca/assets/photos/demo1.png)**

## RCC access/settings keys

- open Registry Editor, and navigate to: ```HKEY_LOCAL_MACHINE -> SOFTWARE -> WOW6432Node``` and go into ROBLOX Corporation and then the Roblox folder. (if they don't exist, create the keys)
- then, inside of the Roblox folder, right click, go into New, and select String Value. Then the name should be AccessKey and set the value to whatever your RccAuthorization value in your appsettings is. (in the ```Roblox/Roblox.Website``` folder)
- then, make another string value named SettingsKey and set the value to whatever you want. after doing this, you should update AllowedQuietGetJson in your appsettings to have RCCServiceYourSettingsKey and just your settings key, replace YourSettingsKey with your actual settings key.
- then, go into ```Roblox/Roblox.Libraries/Json``` and rename the ```RCCServiceBubbleRev2021RCCIsSoTuff.json``` or whatever it is to ```RCCServiceYourSettingsKey.json``` (replace YourSettingsKey with your settings key, once again).
- you should also do the same thing in the ```Roblox/Roblox.Website/Controllers/Internal/Other/ClientData.cs``` and do a search for ```/v1/settings/application```. Look for the ```RCCServiceBubbleRev2021RCCIsSoTuff``` and replace it with ```RCCServiceYourSettingsKey``` replacing YourSettingsKey with your actual settings key

## RCC public keys
- you need to run the Generate.py file in the ```Roblox/Roblox.Website/RSA``` folder. this will generate the private and public keys. (make sure you have Python installed and the pip package ```cryptography``` installed!)
## 2016 and 2018:
- open the RCCService folder, and drag the RCCService.exe file into HXD. then do ctrl + f and type BGIAA (direction: All)
- get your generated public key from the script you ran earlier, the file is  ```PublicKey2016.pub```. open in notepad, and go back to RCCService in HXD, then replace everything in HXD from BGIAA to the last = sign, then save the file.
- for 2018, you might see multiple keys. just replace the first one.
## 2020
- on 2020, the process is a bit different. open your RCCService.exe file in the RCCService2020 folder in HXD, and do a search for MIIBI (direction: All)
- open your ```PublicKey2020.pub``` in HXD as well. copy everything in it, then go back to your RCCService tab, and paste everything inside from the first - at BEGIN PUBLIC KEY to the last - at END PRIVATE KEY. (there may be a period at the end, if it asks if you want to change the file size, just click cancel, and select one extra dot at the end and it should work)
- **you will see multiple public keys. replace EVERY one for it to work, i don't know why it doesn't work when it only has one replaced, but you have to replace every one.**
## CLIENT
- the client process is the same thing as RCC, just on your client EXE. (except for 2020, you only have to update 1 public key)

## the site should be setup at this point!

- go into your main folder, run ```runall.bat```, and when everything starts, go to your site at ```http://localhost```.
- sign up for an account with the name ```ROBLOX``` or whatever you want, does not matter. then go to /admin, and go to create player under Users, put ID 2500, the name as ```UGC``` and a random password, then go to that user on the admin panel and click Nullify Password.
**if you sign up and it takes around 15-20 seconds and throws an error, try just logging in with the account you created. this happens because it cannot render your avatar, so this will not happen once your renderer is fixed.**
- go back to Create Player and set the ID to 12, and the name as ```BadDecisions```, make the password hard or do Nullify Password on it.
- now, sign up with your account normally.

**congrats, site is setup and made!**

## client (game join)

- go to /game/get-join-script?placeid=(the place you want to join)
- then use Command Prompt and use CD to go into the client's directory in Command Prompt, then do CLIENTNAME.exe (paste everything in the get join script endpoint after the client exe)
  
<div align="center">
    <p>
      <h1>FAQ/common errors</h1>
    </p>
</div>

## 2018/2020 RCC crashes when i try to join a game/render an R15 avatar, what do i do:
- one of the reasons is that you do not have 3d acceleration or DirectX installed. try installing DirectX, this should solve the issue.
## the renderer does StartProcessException... after it does ThumbnailGenerator::click:
- you do not have 3d acceleration. you can try using [Mesa](https://github.com/lightningterror/Mesa3D-Windows/releases/tag/mesa-21.0.3) (32 bit) and putting it in the directory of RCCService. if you already have it, you may not have PlatformContent or shaders.
## when i try to join a game, it says ID = 17:
- this COULD be one of many things: you have not forwarded your allowed network ports through your router/firewall, or updated your GSIPAddress (your/your servers public IP) in the appsettings. you should also check if your RCC is starting successfully, if it is and it loads the game, then it's a port/ip issue or something else
## it says CURL error and prints a bunch of text, ini and dmp files, or says 400 Bad Request a bunch of times:
- go to ```%localappdata%\Roblox\logs``` and delete everything, this happens when there are dump files in the directory
## i will add more here if more people have issues.

higu
