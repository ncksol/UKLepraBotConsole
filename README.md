# UK Lepra Chat Telegram Bot

Victorious, happy and glorious since 1707 by [@ncksol](https://t.me/ncksol)

[Join chat](https://t.me/joinchat/Cqj8dz-ls-_DnfPvNHacKg)

## Build & Deploy
### Build
1. Build and publish ubuntu project:

   `dotnet publish -c release -r linux-x64 --no-self-contained`

2. Create docker image:

   `docker build -t ncksol/ukleprabot:0.4 .`

3. Push docker image:

   `docker image push ncksol/ukleprabot:0.4`

### Deploy
1. Pull image:

   `docker pull ncksol/ukleprabot:0.4`

2. Update service with new image:

   `docker service update --image ncksol/ukleprabot:0.4 bot`

   Or create new service:

   `docker service create --name bot --mount type=volume,source=botdata,destination=/app/Data ncksol/ukleprabot:0.4`
