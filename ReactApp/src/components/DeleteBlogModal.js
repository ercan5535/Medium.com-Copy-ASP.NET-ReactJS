import Button from 'react-bootstrap/Button';
import React, {useState, useEffect} from 'react'
import { useNavigate } from "react-router-dom";
import Modal from 'react-bootstrap/Modal';

  
export default function DeleteBlogModal({handleDelete, isOpen, onClose}) {


    return (
        <> 
          <Modal show={isOpen} onHide={onClose}>
            <Modal.Header closeButton>
              <Modal.Title>Delete Blog</Modal.Title>
            </Modal.Header>
            <Modal.Body>Are you sure you want to continue with your action?</Modal.Body>
            <Modal.Footer>
              <Button variant="outline-secondary" onClick={onClose}>
                Close
              </Button>
              <Button variant="outline-danger" onClick={handleDelete}>
                Delete
              </Button>
            </Modal.Footer>
          </Modal>
        </>
      );
    }