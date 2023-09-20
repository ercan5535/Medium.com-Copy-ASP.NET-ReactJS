# Description

It is a medium.com copy with ASP.NET micro services and React JS.<br>
Cookies are used for JWT Token operations.
<br>
<br>

<div align="center">
  <img src="https://github.com/ercan5535/full-project/assets/67562422/20351d4b-a12a-43ac-88a4-1d6522b065f7" width="500" height="400">
</div>
<br>

### API Gateway
- Serves static files
- Redirects reqeusts to relevant service

### Auth Service
- Responsible for User Register/Login/Logut
- Responsible for Create/Refresh/Store/Blacklist JWT
- JWT parameters defined on settings.py
```
JWT = {
  "ACCESS_TOKEN_LIFETIME": timedelta(minutes=5),
  "REFRESH_TOKEN_LIFETIME": timedelta(days=1),
  "ALGORITHM": "HS256",
  "SIGNING_KEY": "MY_SIGNING_KEY_123",
}
```
- JWT operations handled by jwt_helper.py

### Transactions Service
- Responsible for CRUD operations.
- Authenticate and Authorize(Only managers accounts can confirm Transactions) by checking tokens on cache

### Cache
- Holds (Token, UserData) pairs
- Holds JWT Access tokens for validation from Transaction Service
- Holds JWT Refresh tokens for blacklisting Refresh tokens

### Front-End
- It is a simple single page application.
- There are 2 versions, First I done with vanilla js then I added react version after I learnt react.
- Login, Register, All Transactions, Add Transaction and Transaction Details Pages are available
- Update, Delete, Confirm operations are can be done on Transaction Details Page

# Usage
```
docker-compose up 
```
command is enough to run all services <br>
NGINX will listen localhost:80 for serving react app <br>
also localhost:80/vanilla/ is up for first version.
