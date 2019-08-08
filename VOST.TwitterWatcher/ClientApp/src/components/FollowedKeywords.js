import React, { Component } from 'react';
import { Button, Form, Input } from 'reactstrap';

export class FollowedKeywords extends Component {
    constructor(props) {
        super(props);
        this.state = {loading: true, keywords: []}

        this.handleChange = this.handleChange.bind(this);
        this.subminNewKeyword = this.subminNewKeyword.bind(this);
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
        fetch("/api/v1/keywords/toggle?keyword=" + encodeURIComponent (keyword), {
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

    subminNewKeyword (e) {
        e.preventDefault();
        fetch ("/api/v1/keywords/new", 
        {
            body: JSON.stringify({ word: this.state.newKeyword, enabled: true }),
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            }
        })
        .then(res => {
            if (res.ok) { this.refreshKeywords(); }
            else { this.setState({error: res.status }); }
        });
    }

    handleChange (e) {
        this.setState({newKeyword: e.target.value});
    }

    render () {

        return <div className="mb-2">
                <span>{this.state.error} </span>
                {this.state.loading ? <p>loading</p> : this.state.keywords.map(k => 
                    <Button key={k.keyword}
                        className="mr-2" 
                        color={k.enabled ? "primary" : "secondary"}
                        onClick={(e) => this.toggle(e, k.keyword)} >{k.keyword}</Button> 
                )}
                <span className="mr-2">
                    <Form onSubmit={this.subminNewKeyword} className="d-inline" style={{ width: "auto" }} >
                        <Input id="newKeyword" className="d-inline" type="text" value={this.state.newKeyword} 
                        onChange={this.handleChange} placeholder="adicionar..." style={{width:"auto"}} />
                    </Form>
                </span>
            </div>

    }

}