import React, {useState, useEffect} from 'react'
import Button from 'react-bootstrap/Button';
import { useNavigate } from "react-router-dom";
import Card from 'react-bootstrap/Card';

export default function BlogCard({Blog}) {
  const navigate = useNavigate();

  function convertDate(inputDate)
  {
      const inputDateObj = new Date(inputDate);
      const options = { year: 'numeric', month: 'short', day: '2-digit' };
      return inputDateObj.toLocaleDateString('en-US', options);
  }

  return (
      <Card className="text-center blog-card" bg="">
        <Card.Body>
          <Card.Title>{Blog.blogTitle}</Card.Title>
          <Card.Text>
          <hr/>
          <div className='card-info-bar'>
                <div>
                  <i className="bi bi-person-circle"> {Blog.blogAuthor}</i>
                  <p>{convertDate(Blog.createdAt)}</p>
                </div>
                <div>
                  <div className='like-container'>
                      <i className="bi bi-hand-thumbs-up"></i>
                      {Blog.likesCount}
                  </div>
                  <div className='like-container'>
                      <i className="bi bi-chat-dots"></i>
                      {Blog.commentsCount}
                  </div>
                </div>
          </div>
          </Card.Text>
          
          <Button onClick={()=> navigate(`/${Blog.blogAuthor}/${Blog.id}`)} variant="secondary">Read Blog</Button>
        </Card.Body>
      </Card>
  );
}