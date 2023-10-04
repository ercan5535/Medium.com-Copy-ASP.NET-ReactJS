import React, {useState, useEffect} from 'react'
import ReactMarkdown from 'react-markdown'

// open new tab after clicking hyperlink
function LinkRenderer(props) {
    console.log({ props });
    return (
      <a href={props.href} target="_blank" rel="noreferrer">
        {props.children}
      </a>
    );
}
  
export default function ContentDisplay({contentItem}) {
    return(
        <div className='content-item-container'>
            {contentItem.type === "text" && <ReactMarkdown components={{ a: LinkRenderer }} className='content-item-markdown'>{contentItem.content}</ReactMarkdown>}
            {contentItem.type === "image" &&  
                <div className='content-item-image'>
                    <img className='item-image'
                        src={contentItem.content.split(",alt=")[0]}
                    />
                    <div className='image-caption'>
                        <p>{contentItem.content.split(",alt=")[1]}</p>
                    </div>
                </div>
            }
        </div>
    )
}
