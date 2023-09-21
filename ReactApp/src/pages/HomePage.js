import React, {useState, useEffect} from 'react'
import BlogCard from '../components/BlogCard'

export default function HomePage (){
    let page = 1;
    let blockRequest = false;
    let isLoading = false;
    const [blogsData, setBlogsData] = useState([])
    
    async function getBlogs(){
        const blog_response = await fetch(
            `http://localhost/blog/api/Blog/meta?page=${page}&pageSize=5`, {
            method: "GET",
            credentials: 'include'
        });
        if (blog_response.ok){
            var responseJson = await blog_response.json()
            var newBlogs = responseJson.data
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

    return (
        <main className='blog-cards-container'>

            {blogsData.map((blog) => {
                return (
                    <BlogCard key={blog.id} Blog={blog} />
                )
            })}
        </main>
    )
}