import React, { useState } from 'react';
import Drawer from '@mui/material/Drawer';
import CommentDisplay from './CommentDisplay';
import Form from 'react-bootstrap/Form';
import Button from 'react-bootstrap/Button';

export default function CommentDrawer ({setBlogData, blogData, loginStatus, isOpen, onClose}) {



    const [comment, setComment] = useState('')

    const addComment = (newComment) => {
      setBlogData((prevState) => ({
        ...prevState, 
        comments: [...prevState.comments, newComment],
      }));
    };
  
    function handleChange(event) {
      const value = event.target.value
      setComment(value)
    }
  
    const handleKeyPress = (event) => {
      if (event.keyCode === 13 && !event.shiftKey) {
        handleSubmit(event);
      }
    };
  
    async function handleSubmit(event) {
      if (!comment){
        return
      }
      event.preventDefault()
      // Convert the data object to a JSON string
      var jsonData = JSON.stringify({"comment": comment});
  
      const comment_response = await fetch(
          `http://localhost/blog/api/Blog/comment/${blogData.id}`, {
          method: "POST",
          credentials: 'include',
          body: jsonData,
          headers: {
              "Content-Type": "application/json"
          }
      });
      if (comment_response.ok){
          var response_json = await comment_response.json()
          const response_data = response_json.data
          addComment(response_data)
          setComment("")
      }
      else{
          const response_json = await comment_response.json();
          console.log(response_json)
      }
    }
  
  

  return (
    <div>
      <Drawer 
        anchor="right" 
        open={isOpen} 
        onClose={onClose} 
        PaperProps={{sx: { width: "23%"}}}
      >
        <div className='drawer-body-block'>
        <h3>Comments({blogData.comments.length})</h3>
            {loginStatus && <>
                <hr/>
                <Form>
                    <Form.Group className="mb-3">
                    <Form.Label>Share your thoughts</Form.Label>
                    <Form.Control 
                        as="textarea" 
                        rows={3}
                        onChange={handleChange} 
                        placeholder='What are your thoughts?'
                        value={comment}
                        onKeyDown={handleKeyPress}
                    />
                    </Form.Group>
                </Form>
                <div className='share-button'>
                    <Button type='submit' onClick={handleSubmit} variant="outline-secondary" size='sm'>Share</Button>
                </div>
            </>} 

            {blogData.comments.map((comment, commentIndex) => {
            return(
                <>
                <CommentDisplay key={commentIndex} comment={comment} setBlogData={setBlogData} blogData={blogData}/>
                </>
            );
            })}
        </div>
      </Drawer>
    </div>
  );
};