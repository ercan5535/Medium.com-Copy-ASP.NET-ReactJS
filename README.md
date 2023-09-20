# Description

It is a medium.com copy with ASP.NET micro services and React JS.<br>
Cookies are used for JWT Token operations.
<br>
<br>

<div>
  <img src="https://github.com/ercan5535/full-project/assets/67562422/20351d4b-a12a-43ac-88a4-1d6522b065f7" width="500" height="400">
</div>

### API Gateway
- Serves static files
- Redirects reqeusts to relevant service

### Auth Service
- Responsible for User Register/Login/Logut
- Responsible for Create/Refresh/Store/Blacklist JWT
- JWT parameters defined on appsettings.json
```
  "AccessTokenLifetime": 5,
  "RefreshTokenLifetime": 60
```
- JWT operations handled by Helpers/JwtHelper.cs

### Blog Service
- Responsible for CRUD operations (blog, like, comment).
- Authenticate requests by checking tokens on cache

### Cache
- Holds (Token, UserData) pairs
- Holds JWT Access tokens for validation from Blog Service
- Holds JWT Refresh tokens for blacklisting Refresh tokens

### Front-End
- It is a simple version of medium.com
- Blogs can be created cell by cell like on medium.com
- react-markdown used for text displaying

# Usage
```
docker-compose up 
```
command is enough to run all services <br>
NGINX will listen localhost:80 for serving react app <br>
