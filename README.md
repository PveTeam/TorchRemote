## Currently client app is not finished

# Api Roadmap

- [x] world
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
- [x] settings
    - [x] get type schema (maybe in graphql or json schema)
    - [x] list properties of type instance
    - [x] get property value
    - [x] set property value
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
- [ ] chat
    - [x] live messages (websocket)
    - [x] invoke command
    - [ ] invoke command (with direct response for optional delay)
    - [x] send message (custom color author channel)
