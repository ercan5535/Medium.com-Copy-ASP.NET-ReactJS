# Description

It is a medium.com copy with ASP.NET micro services and React JS.<br>
Cookies are used for JWT Token operations.
<br>
<br>

<div>
  <img src="https://github.com/ercan5535/full-project/assets/67562422/2f3b78dd-4497-45f6-92a7-6e8bd593c80c" width="500" height="400">
</div>

### API Gateway
- Serves static files
- Redirects reqeusts to relevant service

### User Service
<div>
  <img src="https://github.com/ercan5535/full-project/assets/67562422/d802e4c4-c991-4bc2-85dd-b4551d784ad7" width="250" >
</div>

- Responsible for User Register/Login/Logut
- Responsible for Create/Refresh/Store/Blacklist JWT
- JWT operations handled by Helpers/JwtHelper.cs
- JWT parameters defined on appsettings.json
```
  "AccessTokenLifetime": 5,
  "RefreshTokenLifetime": 60
```

### Blog Service
<div>
  <img src="https://github.com/ercan5535/full-project/assets/67562422/a1a38e2a-eb23-4b44-b7e3-96ab46ae99b7" width="250" >
</div>

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
