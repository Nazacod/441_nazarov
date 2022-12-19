// (function init () {
//     let b_clear = document.getElementById('b_clear')
//     b_clear.addEventListener('click', clear)
//     let b_select = document.getElementById('b_select')
//     b_select.addEventListener('click', select)
//     let b_download = document.getElementById('b_download')
//     b_download.addEventListener('click', download)
//     let b_post = document.getElementById('b_post')
//     b_post.addEventListener('click', post)
//     console.log(`Hello`)
// })()

var host = "http://localhost:5254"

let b_clear = document.getElementById('b_clear')
b_clear.addEventListener('click', clear)
let b_select = document.getElementById('b_select')
b_select.addEventListener('click', select)
let b_download = document.getElementById('b_download')
b_download.addEventListener('click', download)
let b_post = document.getElementById('b_post')
b_post.addEventListener('click', post)
console.log(`Hello`)

async function clear(event) {
    try {
        let wrapper = document.getElementsByClassName('wrapper')[0]
        wrapper.innerHTML = ''
        let item_wrapper = document.createElement('div')
        item_wrapper.setAttribute('class', 'photo_item')
        let img = document.createElement('img')
        img.setAttribute('src', 'blank.jpg')
        let p = document.createElement('p')
        p.setAttribute('class', 'desc')
        p.innerHTML = 'Blank'
        item_wrapper.appendChild(img)
        item_wrapper.appendChild(p)
        wrapper.appendChild(item_wrapper)
    }
    catch (error) {
        console.log(`error: ${error}`)
    }
}

async function select(event) {
    try {
        const files = document.getElementById('files')
        files.click()
    }
    catch (error) {
        console.log(`error: ${error}`)
    }
}

async function download(event) {
    try {
        let url = "{0}/images/".replace('{0}', host)
        let response = await fetch(url)
        let images_ids = await response.json()

        let wrapper = document.getElementsByClassName('wrapper')[0]
        wrapper.innerHTML = ''

        for (let x in images_ids) {
            let url_id = url + images_ids[x]
            let response_id = await fetch(url_id)
            let image_info = await response_id.json()
            
            let item_wrapper = document.createElement('div')
            item_wrapper.setAttribute('class', 'photo_item')
            
            let img = document.createElement('img')
            img.setAttribute('src', 'data:image/png;base64,' + image_info.value.data)

            let p = document.createElement('p')
            p.setAttribute('class', 'desc')
            p.innerHTML = image_info.filename

            item_wrapper.emotions = image_info.emotions

            item_wrapper.appendChild(img)
            item_wrapper.appendChild(p)

            item_wrapper.addEventListener('click', display_emotions)

            wrapper.appendChild(item_wrapper)
        }
    }
    catch (error) {
        console.log(`error: ${error}`)
    }
}

async function display_emotions(event) {
    try {
        metrics = document.getElementsByClassName('metrics')[0]
        let emotions_target = event.currentTarget.emotions
        let output_string = ''
        for (let x in emotions_target){
            output_string += emotions_target[x].name + ':' + emotions_target[x].value + '<br>'
        }
        metrics.innerHTML = output_string
    }
    catch (error) {
        console.log(`error: ${error}`)
    }
}

async function post(event) {
    try {
        const files = document.getElementById('files').files
        for (let x of files){
            let prms = new Promise(res => {
                let reader = new FileReader()
                reader.onloadend = e => res(e.target.result)
                reader.readAsDataURL(x)
            })
            let img = (await prms).split(',')[1]
            const post_request = {
                mode: 'cors',
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(img)
            }
            let url = "{0}/images".replace('{0}', host)
            let response = await fetch(url, post_request)
            console.log(response.json())
        }
            
        
    }
    catch (error) {
        console.log(`error: ${error}`)
    }
}