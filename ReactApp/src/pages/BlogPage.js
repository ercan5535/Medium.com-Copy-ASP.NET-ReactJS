import React, {useState, useEffect} from 'react';
import { useNavigate, useParams } from "react-router-dom";
import ContentDisplay from '../components/ContentDisplay';
import ReactMarkdown from 'react-markdown';
import Button from 'react-bootstrap/Button';
import Dropdown from 'react-bootstrap/Dropdown';
import ContentInput from '../components/ContentInput';
import DeleteBlogModal from '../components/DeleteBlogModal';
import CommentDrawer from '../components/CommentDrawer';

export default function BlogPage ({loginStatus, setAlertMessage}){
    let { id } = useParams();
    const navigate = useNavigate();
    const imageInputRef = React.createRef();
    
    const [blogData, setBlogData] = useState()
    const [loading, setLoading] = useState(true)
    const [createdDate, setCreatedDate] = useState("")
    const [liked, setLiked] = useState(false)
    const [updatePage, setUpdatePage] = useState(false)
    const [isModalOpen, setModalOpen] = useState(false);
    const [isDrawerOpen, setDrawerOpen] = useState(false);    
    let userData = JSON.parse(localStorage.getItem("userData"))
    if (userData == null)
    {
        userData = {"userName": "", "userId":0}
    }

    function convertDate(inputDate)
    {
        const inputDateObj = new Date(inputDate);
        const options = { year: 'numeric', month: 'short', day: '2-digit' };
        return inputDateObj.toLocaleDateString('en-US', options);
    }

    async function getBlog(){
        const blog_response = await fetch(
            `http://localhost/blog/api/Blog/${id}`, {
            method: 'GET',
            credentials: 'include',
            });
        if (blog_response.ok){
            var response_json = await blog_response.json()
            var blogResponseData = response_json.data
            setCreatedDate(convertDate(blogResponseData.createdAt))
            setBlogData(blogResponseData)
            console.log(blogResponseData.likes)
            if (userData)
            {
                //if (blogResponseData.likes.includes(userData.userId))
                if (blogResponseData.likes.includes(userData.userName))
                {
                    setLiked(true)
                }
            }
            setLoading(false)
            console.log(blogResponseData)
        }
    }

    async function handleLike(){
        const like_response = await fetch(
            `http://localhost/blog/api/Blog/like/${id}`, {
            method: 'POST',
            credentials: 'include',
        });
        if (like_response.ok)
        {
            // update liked state
            setLiked((prevState) => !prevState)
            // update blogData likes 
            const updatedBlogData = { ...blogData };
            updatedBlogData.likes.push(userData.userName);
            setBlogData(updatedBlogData);
        }
    }

    async function handleUnLike(){
        const unlike_response = await fetch(
            `http://localhost/blog/api/Blog/unlike/${id}`, {
            method: 'POST',
            credentials: 'include',
        });        
        if (unlike_response.ok)
        {
            // update liked state
            setLiked((prevState) => !prevState)
            // update blogData likes 
            const updatedBlogData = { ...blogData };
            const indexToRemove = updatedBlogData.likes.indexOf(userData.userName);
            if (indexToRemove !== -1) {
            updatedBlogData.likes.splice(indexToRemove, 1);
            }
            setBlogData(updatedBlogData);
        }
    }

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
 
        try{
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
        }
        catch{
            console.log("image uploading is cancelled")
        }
    };

    async function handleUpdate(event){
        event.preventDefault()
        // Convert the data object to a JSON string
        
        var jsonData = JSON.stringify(blogData.blogContent);

        const update_response = await fetch(
            `http://localhost/blog/api/Blog/${blogData.id}`, {
            method: "PUT",
            credentials: 'include',
            body: jsonData,
            headers: {
                "Content-Type": "application/json"
            }
        });
        console.log(update_response)
        if (update_response.ok){
            var response_json = await update_response.json();
            const response_data = response_json.data
            console.log(response_data)
            setUpdatePage(false);
            setAlertMessage("Your Blog Updated!"); 
        }
        else{
            const response_data2 = await update_response.text();
            console.log(response_data2)
        }
    }

    async function handleDelete(){
        const delete_response = await fetch(
            `http://localhost/blog/api/Blog/${blogData.id}`, {
            method: "DELETE",
            credentials: 'include',
            headers: {
                "Content-Type": "application/json"
            }
        });
        console.log(delete_response)
        if (delete_response.ok){
            setAlertMessage("Your Blog Deleted!");
            navigate("/");
        }
        else{
            const response_data2 = await delete_response.text();
            console.log(response_data2)
        }
    }

    useEffect(() => {
        getBlog()
    }, [])

    return (
        <main>
            {loading && <h1>Loading</h1>}
            {!loading && <>
                <div className='content-block'>
                    <ReactMarkdown className='content-item-text'>{"# " + blogData.blogTitle}</ReactMarkdown>

                    <hr className='horizontal-line' />
                    <div className='info-bar'>
                        <div className='first-info'>
                            <i className="bi bi-person-circle"> {blogData.blogAuthor}</i>
                            {createdDate}
                        </div>
                        <div className='second-info'>
                            <div className='like-container'>
                                {!loginStatus && <i className="bi bi-hand-thumbs-up"></i>}
                                {liked && loginStatus && <i onClick={handleUnLike} className="bi bi-hand-thumbs-up-fill clickable"></i>}
                                {!liked && loginStatus && <i onClick={handleLike} className="bi bi-hand-thumbs-up clickable"></i>}
                                {blogData.likes.length}
                            </div>
                            <div className='like-container' onClick={() => setDrawerOpen(true)}>
                                <i className="bi bi-chat-dots clickable"></i>
                                {blogData.comments.length}
                            </div>
                        </div>
                    </div>
                    <hr className='horizontal-line' />

                    {!updatePage && <>
                        {blogData.blogAuthor === userData.userName && <div className='updel-buttons'>
                            <Button variant="outline-secondary" onClick={() => setUpdatePage(true)} size='sm'>Update</Button>
                            <Button variant="outline-secondary" onClick={() => setModalOpen(true)} size='sm'>Delete</Button>
                            <DeleteBlogModal handleDelete={handleDelete} isOpen={isModalOpen} onClose={() => setModalOpen(false)}/>
                        </div>    
                        }

                        {blogData.blogContent.map((contentItem, contentIndex) => {
                            return (
                                <ContentDisplay key={contentIndex} contentItem={contentItem}/>
                            )
                        })}
                    </>}

                    {updatePage && <>
                        {blogData.blogContent.map((contentItem, contentIndex) => {
                            return <ContentInput key={contentIndex} contentItem={contentItem} index={contentIndex} blogData={blogData} setBlogData={setBlogData}/>
                        })}
                    
                        <Dropdown className='new-content'>
                            <Dropdown.Toggle variant="light" id="dropdown-basic" className='rounded-circle'>
                                +
                            </Dropdown.Toggle>

                            <Dropdown.Menu>
                                <Dropdown.Item onClick={handleAddTextInput}>Text</Dropdown.Item>
                                <Dropdown.Item onClick={() => imageInputRef.current.click()}>Image</Dropdown.Item>
                            </Dropdown.Menu>
                        </Dropdown>
                        <br/>

                        <Button onClick={handleUpdate} variant="light">Update Blog</Button>

                        <input
                            type="file"
                            accept="image/*"
                            style={{ display: 'none' }}
                            ref={imageInputRef}
                            onChange={handleAddImageInput}
                        />
                    </>}
                </div>

                <CommentDrawer 
                    setBlogData={setBlogData}
                    blogData={blogData}
                    loginStatus={loginStatus}
                    isOpen={isDrawerOpen}
                    onClose={() => setDrawerOpen(false)}
                />
            </>}


        </main>
    )
}