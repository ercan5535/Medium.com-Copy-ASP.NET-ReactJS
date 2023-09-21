import React, {useState, useEffect} from 'react'
import {useParams} from "react-router-dom";
import ContentDisplay from '../components/ContentDisplay';
import ReactMarkdown from 'react-markdown';


export default function BlogPage ({loginStatus}){
    let { id } = useParams();
    const [blogData, setBlogData] = useState()
    const [loading, setLoading] = useState(true)
    const [createdDate, setCreatedDate] = useState("")
    const [liked, setLiked] = useState(false)
    
    const userData = JSON.parse(localStorage.getItem("userData"))

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
            setLoading()
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

    useEffect(() => {
        getBlog()
    }, [])

    return (
        <main>
            {loading && <h1>Loading</h1>}
            {!loading && <>

                <h1>Blog Page {id}</h1>
                
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
                                {!loginStatus && <i className="bi bi-hand-thumbs-up-fill"></i>}
                                {liked && loginStatus && <i onClick={handleUnLike} className="bi bi-hand-thumbs-up-fill clickable"></i>}
                                {!liked && loginStatus && <i onClick={handleLike} className="bi bi-hand-thumbs-up clickable"></i>}
                                {blogData.likes.length}
                            </div>
                            <div className='like-container'>
                                <i className="bi bi-chat-dots clickable"></i>
                                {blogData.comments.length}
                            </div>
                        </div>
                    </div>
                    <hr className='horizontal-line' />
                    {blogData.blogContent.map((contentItem, contentIndex) => {
                        return (
                            <ContentDisplay contentItem={contentItem} key={contentIndex}/>
                        )
                    })}
                </div>
            </>}
        </main>
    )
}