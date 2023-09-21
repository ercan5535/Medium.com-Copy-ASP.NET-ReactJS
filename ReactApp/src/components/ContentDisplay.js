import React, {useState, useEffect} from 'react'
import ReactMarkdown from 'react-markdown'


export default function ContentDisplay({contentItem}) {
    return(
        <div className='content-item-container'>
            {contentItem.type === "text" && <ReactMarkdown className='content-item-markdown'>{contentItem.content}</ReactMarkdown>}
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
