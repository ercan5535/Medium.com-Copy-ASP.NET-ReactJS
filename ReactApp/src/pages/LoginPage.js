import React, {useState} from 'react'

export default function LoginPage({setLoginStatus}){
    const [errorMessage, setErrorMessage] = useState('');
    const [formData, setFormData] = useState(
        {
            userName: "",
            password: "",
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
        // Convert the data object to a JSON string
        var jsonData = JSON.stringify(formData);

        const login_response = await fetch(
            "http://localhost/auth/api/Login", {
            method: "POST",
            credentials: 'include',
            body: jsonData,
            headers: {
                "Content-Type": "application/json"
            }
        });
        console.log(login_response)
        if (login_response.ok){
            var response_json = await login_response.json()
            const userData = response_json.data
            localStorage.setItem("userData", JSON.stringify(userData))
            setLoginStatus(true)
        }
        else{
            var response_json = await login_response.json()
            setErrorMessage(response_json.message); // Update error message
        }
    }

    return (
        <main onClick={() => setErrorMessage('')}>
            <div className="col-md-6 offset-md-3">
                <br />
                {errorMessage && <div className='error-message'>{errorMessage}</div>}
                <br />
                <form onSubmit={handleSubmit}>
                <h1>Login</h1>
                <br />
                <div className="form-group">
                    <label htmlFor="LoginUsername">Username</label>
                    <input 
                        type="text" 
                        className="form-control" 
                        id="LoginUsername" 
                        placeholder="Enter username"
                        onChange={handleChange}
                        name='userName'
                    />
                </div>
                <br/>
                <div className="form-group">
                    <label htmlFor="LoginPassword1">Password</label>
                    <input 
                        type="password" 
                        className="form-control" 
                        id="LoginPassword1" 
                        placeholder="Password"
                        onChange={handleChange}
                        name='password'
                    />
                </div>
                <br/>
                <button type="submit" className="btn btn-secondary">Login</button>
                </form>
            </div>
        </main>
    )
}