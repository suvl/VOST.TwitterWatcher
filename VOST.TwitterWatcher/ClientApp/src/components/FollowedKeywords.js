import React, { Component } from 'react';
import { Button } from 'reactstrap';

export class FollowedKeywords extends Component {
    constructor(props) {
        super(props);
        this.state = {loading: true, keywords: []}
    }

    componentDidMount () {
        this.refreshKeywords();
    }

    refreshKeywords () {
        fetch("/api/v1/keywords")
            .then(res => res.json())
            .then(data => {
                this.setState({ loading: false, keywords: data });
            });
    }

    toggle (event, keyword) {
        event.preventDefault();
        fetch("/api/v1/keywords/toggle?keyword=" + keyword, {
            method: 'PATCH',
            headers: {
                'Accept': 'application/json'
            }
        })
        .then(res => {
            if (res.ok)
            {
                this.refreshKeywords();
            } else {
                this.setState({error: res.status})
            }
        })
    }

    render () {

        return <div className="mb-2">
                <span>{this.state.error} </span>
                {this.state.loading ? <p>loading</p> : this.state.keywords.map(k => 
                    <Button 
                        className="mr-2" 
                        color={k.enabled ? "primary" : "secondary"}
                        onClick={(e) => this.toggle(e, k.keyword)} >{k.keyword}</Button> 
                )}
            </div>

    }

}