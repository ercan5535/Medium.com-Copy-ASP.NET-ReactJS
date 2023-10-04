import React, {useState, useEffect} from 'react'
import Button from 'react-bootstrap/Button';
import ReactMarkdown from 'react-markdown'


export default function ContentInput({contentItem, index, handleChangeText, blogData, setBlogData}) {
    const [preview, setPreview] = useState(false)

    function handleChangeText(event) {
        const value = event.target.value
        const index = event.target.getAttribute('content-index');
        console.log(value, index)

        // Update the state with the new data
        const updatedBlogData = { ...blogData };
        updatedBlogData.blogContent[index].content = value;
        setBlogData(updatedBlogData);
        }

    function handleChangeCaption(event){
        const value = event.target.value
        const index = event.target.getAttribute('content-index');
        console.log(value, index)

        // Update the state with the new data
        const updatedBlogData = { ...blogData };
        let imageString = updatedBlogData.blogContent[index].content.split(",alt=")[0]
        updatedBlogData.blogContent[index].content = imageString + ",alt=" + value
        setBlogData(updatedBlogData);
    } 
   
    
    function moveItemUp(index){
        // copy state
        const updatedBlogData = { ...blogData };
        let arr = updatedBlogData.blogContent;
        // switch content itemts
        let el = arr[index];
        arr[index] = arr[index - 1];
        arr[index - 1] = el;
        // update state
        setBlogData(updatedBlogData);

    }

    function moveItemDown(index){
        // copy state
        const updatedBlogData = { ...blogData };
        let arr = updatedBlogData.blogContent;
        // switch content itemts
        let el = arr[index];
        arr[index] = arr[index + 1];
        arr[index + 1] = el;
        // update state
        setBlogData(updatedBlogData);
    }

    function deleteItem(index){
        // copy state
        const updatedBlogData = { ...blogData };
        updatedBlogData.blogContent.splice(index, 1)
        setBlogData(updatedBlogData);
    }

    if (contentItem.type === "text")
    {
        return (
            <div className='content-item-container'>
                {!preview && <textarea
                    className='content-item-text'
                    key={index}
                    value={contentItem.content}
                    content-index={index}
                    onChange={handleChangeText}
                    placeholder='Enter Your Markdown Here'
                />}
                {preview && <ReactMarkdown source className='content-item-markdown'>{contentItem.content}</ReactMarkdown>}
                <div className='content-item-buttons'>
                    {index!==0 && <Button title='Move Up' onClick={() => moveItemUp(index)} variant="light" size="sm" className='rounded-circle'>/\</Button>}
                    <Button title='Delete' onClick={() => deleteItem(index)} variant="light" size="sm" className='rounded-circle'>X</Button>
                    {index!==(blogData.blogContent.length-1) && <Button title='Move Down' onClick={() => moveItemDown(index)} variant="light" size="sm" className='rounded-circle '>\/</Button>}
                    {preview && <Button title='Edit' onClick={() => setPreview(false)} variant="light" size="sm" className='rounded-circle'><i class="bi bi-pencil-square"></i></Button>}
                    {!preview && <Button title='Preview' onClick={() => setPreview(true)} variant="light" size="sm" className='rounded-circle'><i class="bi bi-eye"></i></Button>}
                </div>
    
            </div>
        )
    }
    else
    {
        return (
            <div className='content-item-container'>
                <div className='content-item-image'>
                    <img className='item-image'
                        key={index}
                        src={contentItem.content.split(",alt=")[0]}
                    />
                    <div className='image-caption'>
                        <textarea
                            placeholder='Type caption for image (optional)'
                            content-index={index}
                            onChange={handleChangeCaption}
                            value={contentItem.content.split(",alt=")[1]}
                        />
                    </div>
                </div>
                
                <div className='content-item-buttons'>
                {index!==0 && <Button title='Move Up' onClick={() => moveItemUp(index)} variant="light" size="sm" className='rounded-circle'>/\</Button>}
                <Button title='Delete' onClick={() => deleteItem(index)} variant="light" size="sm" className='rounded-circle'>X</Button>
                {index!==(blogData.blogContent.length-1) && <Button title='Move Down' onClick={() => moveItemDown(index)} variant="light" size="sm" className='rounded-circle'>\/</Button>}
                {<Button  style={{visibility: "hidden"}} size="sm"><i class="bi bi-pencil-square"></i></Button>}

                </div>
            </div>

        )
    }
}
