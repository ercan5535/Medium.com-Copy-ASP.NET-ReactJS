import { useNavigate, Link } from 'react-router-dom';
import Container from 'react-bootstrap/Container';
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import NavDropdown from 'react-bootstrap/NavDropdown';
import Form from 'react-bootstrap/Form';
import Button from 'react-bootstrap/Button';
import InputGroup from 'react-bootstrap/InputGroup';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';


export default function NavigationBar({loginStatus}){
    const navigate = useNavigate();
    var userData = JSON.parse(localStorage.getItem("userData"))

    function handleSearchSubmit(event) {
      event.preventDefault();
      const searchQuery = event.target.elements.q.value;
      navigate(`/search-blog/${searchQuery}`);
    }

    async function logout_request(){
        const logout_response = await fetch(
            "http://localhost/auth/api/logout", {
            method: "DELETE",   
            credentials: 'include',
        });                     
        if (logout_response.ok){
            // Reload page      
            window.location.reload();
            // Delete user data from local storage
            localStorage.removeItem("userData")
        }                                  
        console.log("logged out")            
    }                                                   
                                                        
  return(                                             
    <Navbar bg="dark" data-bs-theme="light" expand="lg" className="bg-body-tertiary" >
    <Container>                                       
      <Navbar.Brand href="/">Bll00g</Navbar.Brand>    
      <Navbar.Toggle  aria-controls="basic-navbar-nav" />
      <Navbar.Collapse className='nav-content'  id="basic-navbar-nav">         
        <Nav className='nav-left' >                     
          {loginStatus && <Link to="/new-blog"  className="nav-link">New Blog</Link>}
          
        </Nav >

        <Nav className='nav-center'> 
          <Form method='get' action='/search-blog' >              
            <Row>                                     
              <Col xs="auto">                         
                <Form.Control                         
                  type="text"                         
                  placeholder="Search Blog"
                  className="mr-sm-2"
                  name="q"
                />                      
              </Col>         
            </Row>                 
          </Form>  
        </Nav>                                   

        <Nav className='nav-right'>
          <NavDropdown title={loginStatus ? userData.userName : "Don't Have Account?"} id="basic-nav-dropdown">
            {
              !loginStatus && <>
                <NavDropdown.Item href="/login">Login</NavDropdown.Item>
                <NavDropdown.Item href="/register">Register</NavDropdown.Item>
              </>
            }
            {
              loginStatus && <>
                <NavDropdown.Item href={`/${JSON.parse(localStorage.getItem("userData")).userName}/blogs`}>My Blogs</NavDropdown.Item>
                <NavDropdown.Divider />
                <NavDropdown.Item onClick={() => logout_request()}>
                  Logout
                </NavDropdown.Item>
              </>
            }
          </NavDropdown>
        </Nav>

      </Navbar.Collapse>
    </Container>
    </Navbar>
    )
  }





//  <Navbar bg="dark" data-bs-theme="dark" expand="lg" className="bg-body-tertiary" >
//  <Container>                                       
//    <Navbar.Brand href="/">Bll00g</Navbar.Brand>    
//    <Navbar.Toggle aria-controls="basic-navbar-nav" />
//    <Navbar.Collapse id="basic-navbar-nav">         
//      <Nav className="me-auto">                     
//        {loginStatus && <Link to="/new-blog" className="nav-link">New Blog</Link>}
//      </Nav >                                       
//                                                    
//                                                    
//                                                    
//      <Nav >                                        
//      <Form inline className="d-flex">              
//          <Row>                                     
//            <Col xs="auto">                         
//              <Form.Control                         
//                type="text"                         
//                placeholder="Search Blog"
//                className=" mr-sm-2"  
//              />                      
//            </Col>                    
//            <Col xs="auto">           
//              <Button variant="secondary" type="submit">Submit</Button>
//            </Col>               
//          </Row>                 
//        </Form>               
//      </Nav>
//
//
//      <Nav>
//        <NavDropdown title={loginStatus ? userData.userName : "Don't Have Account?"} id="basic-nav-dropdown">
//
//          {!loginStatus && <><NavDropdown.Item href="/login">Login</NavDropdown.Item>
//          <NavDropdown.Item href="/register">Register</NavDropdown.Item></>}
//          {loginStatus && <><NavDropdown.Item href="/my-blogs">My Blogs</NavDropdown.Item>
//          <NavDropdown.Divider />
//          <NavDropdown.Item onClick={() => logout_request()}>
//            Logout
//          </NavDropdown.Item></>}
//        </NavDropdown>
//      </Nav>
//      </Navbar.Collapse>
//  </Container>
//  </Navbar>  