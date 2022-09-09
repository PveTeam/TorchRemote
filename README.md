## Currently client app is not finished

# Api Roadmap

- [ ] world
    - [x] list available (guids)
    - [x] get basic info (name, size)
    - [x] selected get
    - [x] selected set
    - [ ] create
- [x] server
    - [x] start
    - [x] stop
    - [x] settings get
    - [x] settings set
    - [x] status (state sim uptime)
- [ ] settings
    - [ ] get type schema (maybe in graphql or json schema)
    - [ ] list properties of type instance
    - [ ] get property value
    - [ ] set property value
- [ ] plugins
    - [ ] list (with ids for config type instance)
- [ ] players
    - [ ] list
    - [ ] kick (with cooldown or not)
    - [ ] disconnect
    - [ ] ban
    - [ ] promote
    - [ ] demote
- [x] logs (websocket)
- [x] chat
    - [x] live messages (websocket)
    - [x] invoke command
    - [x] send message (custom color author channel)