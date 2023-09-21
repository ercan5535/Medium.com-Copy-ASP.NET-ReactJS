import React, {useState, useEffect} from 'react'
import { useLocation } from 'react-router-dom';
import BlogCard from '../components/BlogCard'

export default function SearchedBlogPage (){
    const location = useLocation();
    const searchParams = new URLSearchParams(location.search);
    const query = searchParams.get('q');

    let page = 1;
    let blockRequest = false;
    let isLoading = false;
    const [blogsData, setBlogsData] = useState([])
    
    async function getBlogs(){
        console.log(page)
        const blog_response = await fetch(
            `http://localhost/blog/api/Blog/meta?page=${page}&pageSize=5&query=${query}`, {
            method: "GET",
            credentials: 'include'
        });
        if (blog_response.ok){
            var response_json = await blog_response.json()
            var newBlogs = response_json.data
            console.log(newBlogs)
            // block future request if there is no new blog
            if(Object.keys(newBlogs).length === 0)
            {   
                console.log("no new post")
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

    
    const handleScroll = () => {
        const threshold = 100
        console.log(window.innerHeight + document.documentElement.scrollTop)
        console.log(document.documentElement.offsetHeight - threshold)
        if (
            !isLoading && 
            (window.innerHeight + document.documentElement.scrollTop >
                document.documentElement.offsetHeight - threshold)
        ) 
        {
            // Define a threshold (e.g., 100 pixels from the bottom)
            isLoading = true;
            if (!blockRequest)
            {
                getBlogs()
                .then(() => {
                    isLoading = false; // Reset the flag when the request is complete
            });
            }
        }
    };

    useEffect(() => {
        getBlogs()
        }, [])

    useEffect(() => {
        window.addEventListener('scroll', handleScroll);
        return () => {
            window.removeEventListener('scroll', handleScroll);
        };
    }, []);
    
    return (
        <main className='content-block'>
        <h1><b>Results for</b> {query} </h1>
        {blogsData.map((blog) => {
        return (
            <BlogCard key={blog.id} Blog={blog} />
            )
        })}
        </main>
    )
}