import React, {useState, useEffect} from 'react';
import Dropdown from 'react-bootstrap/Dropdown';
import Form from 'react-bootstrap/Form';
import Button from 'react-bootstrap/Button';
import DeleteCommentModal from './DeleteCommentModal'


export default function CommentDisplay ({setBlogData, blogData, comment}) {
  const [update, setUpdate] = useState(false)
  const [commentState, setCommentState] = useState(comment.comment)
  const [isModalOpen, setModalOpen] = useState(false);

  let userData = JSON.parse(localStorage.getItem("userData"))
  if (userData == null)
  {
      userData = {"userName": "", "userId":0}
  }

  const CustomToggle = React.forwardRef(({ children, onClick }, ref) => (
    <a
      href=""
      ref={ref}
      onClick={e => {
        e.preventDefault();
        onClick(e);
      }}
    >
      {<i className="bi bi-three-dots-vertical blacked"></i>}
      {children}
    </a>
  ));

  function convertDate(inputDate)
  {
      const backendDate = new Date(inputDate);
      const currentDate = new Date();
      const timeDifference = currentDate - backendDate;

      const minute = 60 * 1000;
      const hour = minute * 60;
      const day = hour * 24;
      const month = day * 30;
      const year = month * 12;
    
      if (timeDifference >= year) {
        const yearsAgo = Math.floor(timeDifference / year);
        return `${yearsAgo} year${yearsAgo > 1 ? 's' : ''} ago`;
      } else if (timeDifference >= month) {
        const monthsAgo = Math.floor(timeDifference / month);
        return `${monthsAgo} month${monthsAgo > 1 ? 's' : ''} ago`;
      } else if (timeDifference >= day) {
        const daysAgo = Math.floor(timeDifference / day);
        return `${daysAgo} day${daysAgo > 1 ? 's' : ''} ago`;
      } else if (timeDifference >= hour) {
        const hoursAgo = Math.floor(timeDifference / hour);
        return `${hoursAgo} hour${hoursAgo > 1 ? 's' : ''} ago`;
      } else {
        const minutesAgo = Math.floor(timeDifference / minute);
        return `${minutesAgo} minute${minutesAgo > 1 ? 's' : ''} ago`;
      }
  }

  function handleChange(event) {
    const value = event.target.value
    setCommentState(value)
  }

  const updateComment = (commentId, newCommentText) => {
    const updatedComments = blogData.comments.map(comment => {
      if (comment.id === commentId) {
        return {
          ...comment,  
          comment: newCommentText 
        };
      }
      return comment;
    });
  
    // Update the state with the new comments array
    setBlogData(prevState => ({
      ...prevState,
      comments: updatedComments
    }));
  };

  const deleteComment = (commentId) => {
    // Use the filter method to create a new array without the comment with the specified ID
    const updatedComments = blogData.comments.filter(comment => comment.id !== commentId);
  
    // Update the state with the new comments array
    setBlogData(prevState => ({
      ...prevState,
      comments: updatedComments
    }));
  };

  async function handleDelete(){
    // Convert the data object to a JSON string
    var jsonData = JSON.stringify({"commentId": comment.id});

    const delete_response = await fetch(
        `http://localhost/blog/api/Blog/comment/${blogData.id}`, {
        method: "DELETE",
        credentials: 'include',
        body: jsonData,
        headers: {
            "Content-Type": "application/json"
        }
    });
    if (delete_response.ok){
         deleteComment(comment.id)
         setModalOpen(false)
    }
    else{
        const response_json = await delete_response.json();
        console.log(response_json)
    }
  }

  async function handleUpdate(){
    if (!comment){
      return
    }
    // Convert the data object to a JSON string
    var jsonData = JSON.stringify({
      "commentId": comment.id,
      "comment": commentState
    });

    const update_response = await fetch(
        `http://localhost/blog/api/Blog/comment/${blogData.id}`, {
        method: "PUT",
        credentials: 'include',
        body: jsonData,
        headers: {
            "Content-Type": "application/json"
        }
    });
    if (update_response.ok){
         updateComment(comment.id, commentState)
         setUpdate(false)
    }
    else{
        const response_json = await update_response.json();
        console.log(response_json)
    }
  }

  const handleKeyPress = (event) => {
    if (event.keyCode === 13 && !event.shiftKey) {
      handleUpdate(event);
    }
  };

 

  return (
    <>
      <hr></hr>
      <div className="comment-info-bar">
          <div>
              <i className="bi bi-person-circle"> {comment.author}</i>
          </div>
          <div className='comment-info-right'>
              {convertDate(comment.createdAt)}
              {userData.userName === comment.author && <>
                <Dropdown >
                  <Dropdown.Toggle as={CustomToggle} id="dropdown-basic">
                  </Dropdown.Toggle>
                  <Dropdown.Menu size="sm" title=""> 
                    <Dropdown.Item onClick={() => setUpdate(true)}>Update</Dropdown.Item>
                    <Dropdown.Item onClick={() => setModalOpen(true)} >Delete</Dropdown.Item>
                    <DeleteCommentModal handleDelete={handleDelete} isOpen={isModalOpen} onClose={() => setModalOpen(false)}/>
                  </Dropdown.Menu>
                </Dropdown>
              </>}
          </div>
      </div>
      <br></br>
      {!update && <pre>{comment.comment}</pre>}
      {update && <>
          <Form onSubmit={handleUpdate}>
              <Form.Group className="mb-3">
              <Form.Control 
                  as="textarea" 
                  rows={3}
                  onChange={handleChange} 
                  placeholder='What are your thoughts?'
                  value={commentState}
                  onKeyDown={handleKeyPress}
              />
              </Form.Group>
          </Form>
      </>}
    </>
  );
}