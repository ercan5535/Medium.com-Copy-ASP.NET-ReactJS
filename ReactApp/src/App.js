import React, {useEffect, useState} from 'react'
import { Routes, Route, Navigate, useLocation } from 'react-router-dom';
import Alert from 'react-bootstrap/Alert';
import NavigationBar from './components/NavigationBar';
import AlertDismissible from './components/AlertDismissible';
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import BlogPage from './pages/BlogPage';
import NewBlogPage from './pages/NewBlogPage';
import UserBlogs from './pages/UserBlogs';
import SearchedBlogPage from './pages/SearchedBlogPage';

function App() {
  const [loginStatus, setLoginStatus] = useState(false)
  const [alertMessage, setAlertMessage] = useState("")
  const location = useLocation();

  async function checkLoginStatus(){
    const access_response = await fetch(
        'http://localhost/auth/api/checkloginstatus', {
        method: 'POST',
        credentials: 'include',
      });
    console.log(access_response)
    if (access_response.ok){
      console.log('Authentication success');
      var response_json = await access_response.json()
      const user_data = response_json.data
      console.log(user_data)
      localStorage.setItem("userData", JSON.stringify(user_data))
      setLoginStatus(true)
    }
    else{
    console.log("Authentication failed")
    setLoginStatus(false)
    }
  }

  useEffect(() => {
    checkLoginStatus()
  }, [location])


  return (
    <main onClick={() => setAlertMessage("")}>
      <NavigationBar loginStatus={loginStatus}/>
      <AlertDismissible alertMessage={alertMessage}/>
      <Routes>
        <Route exact path="/" element = {<HomePage/>}/>
        <Route path="/:user/:id" element={<BlogPage loginStatus={loginStatus}/>}/>
        <Route path="/search-blog" element={<SearchedBlogPage/>}/>
        <Route path="/new-blog" element={loginStatus ? <NewBlogPage setAlertMessage={setAlertMessage}/> : <Navigate to="/"/>}/>
        <Route path="/:user" element={<UserBlogs/>}/>
        <Route path="/login" element = {!loginStatus ? <LoginPage setLoginStatus={setLoginStatus}/> : <Navigate to="/"/>}/>
        <Route path="/register" element = {!loginStatus ? <RegisterPage setAlertMessage={setAlertMessage}/> : <Navigate to="/"/>}/>
      </Routes>
    </main>
  );
}

export default App;
