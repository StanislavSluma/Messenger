# TipaMessenger
## Bekarev Stanislav, 253505
# Tipamessenger app
TipaMessenger is a client-server application that allows you to exchange messages.
The main purpose of this application is to exchange messages between two users (#сумасойти).
But also users are able to create group chats and exchange messages with more than 2 peoples (#обалдеть).
Application includes two parts: server and client. The first one is responsible for establishing
connection between users, proccesing messages and managing database. It also implements buisness logic such as for filtering, searching, adding or deleting messages.
It's also important to implement authorisation service???
The another is used to display received messages with emojis or not in the user interface, allows users to send them.
Дополнительно i want to add emojies благодаря которым users can express their feelings.
# Functionality
1. User registartion (Login + Password => Create User with Id; +JWT Token(Hash.Login,Id))
2. Authorisation (Creates JWT token and stores in private storage. After each user's request, we send with request this token)
3. Creating or deleting a group chat
4. Sending message(to user or to group chat)
5. Deleting message(from user's or group chat)
6. ?Editing message(in user's or group chat)? 
7. Recieving message(some handler or listener?)
8. Set(/unset) reactions on messages
9. Blocking users?
# class diagram
![изображение](https://github.com/StanislavSluma/Messenger/assets/121519412/b81efc77-5398-46a4-991c-06ec0c6b9822)
# Data models
1. User:
 - unique userId for identification
 - password for authorisation
 - username
2. Message:
 - unique messageId for searching in db
 - messageText used to store message
 - list of reactions in order to show emojies under message text. Each reaction would have its own entry in the database, linked to the corresponding message.
 - reciever (Chat entity). This field represents the user who is the intended recipient of the message(stored in db)
 - sender (User entity)  This field represents the user who sent the message(also in db)
 - time - the time when the message was sent. (DateTime)
3. Chat:
 - unique chatId
 - chatName
 - list of participants, who are part of the chat. Each participant would typically be linked to a User entity in the database, 
4. MessageReaction:
 - EmojiName - to display correct emoji(mb id)
 - Unique MesssageReactionId
 - MessageId - to connect with neccesary message
 - UserId - to show who reacted
