import React, {useState, useEffect} from 'react'
import BlogCard from '../components/BlogCard'
import {useParams} from "react-router-dom";

export default function UserBlogsPage(){
    let { user } = useParams();
    var page = 1;
    var blockRequest = false
    const [blogsData, setBlogsData] = useState([])
    async function getBlogs(){
        console.log(page)
        const blog_response = await fetch(
            `http://localhost/blog/api/Blog/meta?page=${page}&pageSize=5&username=${user}`, {
            method: "GET",
            credentials: 'include'
        });
        if (blog_response.ok){
            var response_json = await blog_response.json()
            var newBlogs = response_json.data
            // block future request if there is no new blog
            if(Object.keys(newBlogs).length === 0)
            {
                blockRequest = true
            }
            else
            {
                setBlogsData((prevBlogs) => [...prevBlogs, ...newBlogs]);
                page = page + 1;
            }
        }
        else{
            const response_data = await blog_response.text();
            console.log(response_data)
        }
    }

    useEffect(() => {
        getBlogs()
      }, [])

    useEffect(() => {
        window.addEventListener('scroll', handleScroll);
        return () => {
            window.removeEventListener('scroll', handleScroll);
        };
    }, []);
    
    const handleScroll = () => {
        if (
            window.innerHeight + document.documentElement.scrollTop ===
            document.documentElement.offsetHeight
        ) 
        {
            if (!blockRequest)
            {
                getBlogs();
            }
        }
    };

    return (
        <main className='content-block'>
            <h1>{`${user}'s Blogs`}</h1>
            {blogsData.map((blog) => {
                return (
                    <BlogCard key={blog.id} Blog={blog} />
                )
            })}
        </main>
    )
}