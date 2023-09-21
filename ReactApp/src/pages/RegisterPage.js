import React, {useState, useEffect} from 'react'
import { useNavigate } from "react-router-dom";

export default function RegisterPage ({setAlertMessage}){
    const navigate = useNavigate();
    const [errorMessage, setErrorMessage] = useState('');
    const [formData, setFormData] = useState(
        {
            userName: "",
            password: ""
        }
    )

    function handleChange(event) {
        const {name, value} = event.target
        setFormData(prevFormData => {
            return {
                ...prevFormData,
                [name]: value
            }
        })
    }

    async function handleSubmit(event){
        event.preventDefault()
        if (formData.password !== formData.password2){
            return setErrorMessage('Passwords not match')
        }

        var registerData = {
            "userName": formData.userName,
            "password": formData.password
          };
      
        // Convert the data object to a JSON string
        var jsonData = JSON.stringify(registerData);

        const register_response = await fetch(
            "http://localhost/auth/api/register/", {
            method: "POST",
            credentials: 'include',
            body: jsonData,
            headers: {
              "Content-Type": "application/json"
            }});
          if (register_response.ok){
            // Display login page
            console.log("registered succesfully")
            navigate("/login");
            setAlertMessage("Registered succesfully"); // Update alert message
          }
          else{
            const response_data = await register_response.text();
            setErrorMessage(response_data); // Update error message
          }
    }

    return (
        <main onClick={() => setErrorMessage('')}>
            <div className="col-md-6 offset-md-3">
                <br />
                {errorMessage && <div className='error-message'>{errorMessage}</div>}
                <br />
                <form onSubmit={handleSubmit}>
                <h1>Register</h1>
                <div className="form-group">
                    <label htmlFor="RegisterUsername">Username</label>
                    <input 
                        type="text" 
                        className="form-control" 
                        id="RegisterUsername" 
                        placeholder="Enter username"
                        onChange={handleChange}
                        name='userName'
                    />
                </div>
                <br/>
                <div className="form-group">
                    <label htmlFor="RegisterPassword1">Password</label>
                    <input 
                        type="password" 
                        className="form-control" 
                        id="RegisterPassword1" 
                        placeholder="Password"
                        onChange={handleChange}
                        name='password'
                    />
                </div>
                <br/>
                <div className="form-group">
                    <label htmlFor="RegisterPassword2">Password Confirmation</label>
                    <input 
                        type="password" 
                        className="form-control" 
                        id="RegisterPassword2" 
                        placeholder="Password Confirmation" 
                        onChange={handleChange}
                        name='password2'
                    />
                </div>
                <br/>
                <button type="submit" className="btn btn-secondary">Register</button>
                </form>
                </div>
        </main>
    )
}