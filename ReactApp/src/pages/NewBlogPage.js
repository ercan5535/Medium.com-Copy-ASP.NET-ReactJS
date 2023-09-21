import React, {useState, useEffect} from 'react'
import Dropdown from 'react-bootstrap/Dropdown';
import Button from 'react-bootstrap/Button';
import ContentInput from '../components/ContentInput';
import { useNavigate } from "react-router-dom";

export default function NewBlogPage ({setAlertMessage}){
    const navigate = useNavigate();
    const imageInputRef = React.createRef();
    const [blogData, setBlogData] = useState({
        blogTitle: "",
        blogAuthor: JSON.parse(localStorage.getItem("userData")).userName,
        blogContent: []
    })

    function handleAddTextInput(event){
        // Append new content
        var inputTemplate = {
            "type": "text",
            "content": "",
        }

         // Update the state with the new template data
        const updatedBlogData = { ...blogData };
        updatedBlogData.blogContent = [...updatedBlogData.blogContent, inputTemplate];
        setBlogData(updatedBlogData);
    }

    function handleAddImageInput(event) {
        var inputTemplate = {
            "type": "image",
            "content": "",
        }
 
        // Get uploaded image
        const selectedFile = event.target.files[0];
        const reader = new FileReader();
        reader.onload = () => {
            const base64String = reader.result;

            // Update the state with the new data
            inputTemplate.content = base64String + ",alt="
            const updatedBlogData = { ...blogData };
            updatedBlogData.blogContent = [...updatedBlogData.blogContent, inputTemplate];
            setBlogData(updatedBlogData);
        };
      
        reader.readAsDataURL(selectedFile);
        };

    function handleChangeTitle(event){
        const value = event.target.value;
        // Update the state with the new data
        const updatedBlogData = { ...blogData };
        updatedBlogData.blogTitle = value;
        setBlogData(updatedBlogData);
    }

    async function postBlog(event){
        event.preventDefault()
        // Convert the data object to a JSON string
        
        var jsonData = JSON.stringify(blogData);

        const create_response = await fetch(
            "http://localhost/blog/api/Blog", {
            method: "POST",
            credentials: 'include',
            body: jsonData,
            headers: {
                "Content-Type": "application/json"
            }
        });
        console.log(create_response)
        if (create_response.ok){
            var response_json = await create_response.json();
            const response_data = response_json.data
            console.log(response_data)
            navigate(`/${JSON.parse(localStorage.getItem("userData")).userName}/${response_data.id}`);
            setAlertMessage("Your Blog Created!"); 
        }
        else{
            const response_data2 = await create_response.text();
            console.log(response_data2)
        }
    }

    return (
        <main>
        <div className='content-block'>
            <textarea
                className='content-item-text'
                placeholder='Title'
                value={blogData.blogTitle}
                onChange={handleChangeTitle}
            />

            {blogData.blogContent.map((contentItem, contentIndex) => {
                return <ContentInput key={contentIndex} contentItem={contentItem} index={contentIndex} blogData={blogData} setBlogData={setBlogData}/>
            })}
           
            <Dropdown>
                <Dropdown.Toggle variant="light" id="dropdown-basic" className='rounded-circle'>
                    +
                </Dropdown.Toggle>

                <Dropdown.Menu>
                    <Dropdown.Item onClick={handleAddTextInput}>Text</Dropdown.Item>
                    <Dropdown.Item onClick={() => imageInputRef.current.click()}>Image</Dropdown.Item>
                </Dropdown.Menu>
            </Dropdown>
            <br/>

            <Button onClick={postBlog} variant="light">Create Blog</Button>

            <input
                type="file"
                accept="image/*"
                style={{ display: 'none' }}
                ref={imageInputRef}
                onChange={handleAddImageInput}
            />
        </div> 
        </main>
    )
}