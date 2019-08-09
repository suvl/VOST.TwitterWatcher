import React, { Component } from 'react';

export class TweetList extends Component {
    constructor(props) {
        super(props);
        this.state = {
            loading: true, 
            tweets: [],
            page: 0,
            pageSize: 100
        }

        this.goForward = this.goForward.bind(this);
        this.goBackwards = this.goBackwards.bind(this);
    }

    componentDidMount() {
        this.update();
    }

    update () {
        let url = "/api/v1/tweets?page=" +
                    this.state.page +
                    "&pageSize=" +
                    this.state.pageSize;

        fetch(url)
            .then(res => res.json())
            .then(data => {
                this.setState({ loading: false, tweets: data });
            });
    }

    goForward (e) {
        e.preventDefault();
        this.setState({page: ++ this.state.page, loading: true})
        this.update();
    }

    goBackwards (e) {
        e.preventDefault();
        this.setState({page: -- this.state.page, loading: true})
        this.update();
    }

    render () {
        if (this.state.loading)
        {
            return <p>Loading...</p>;
        } else {
            return <div>
                <table className='table table-striped'>
                    <thead>
                        <tr>
                            <th>Time</th>
                            <th>Tweet</th>
                            <th>User</th>
                            <th>Keywords</th>
                            <th>Location</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        {this.state.tweets.map(t => 
                            <tr key={t.id}>
                                <td>{t.createdAt}</td>
                                <td>{t.text}</td>
                                <td>{t.userHandle}</td>
                                <td>{t.matchedKeywords.join(", ")}</td>
                                <td>{t.location}</td>
                                <td>
                                    <a href={"https://twitter.com/"+t.userHandle+"/status/"+t.id}>
                                        <i className="fab fa-twitter"/>
                                    </a>
                                </td>
                            </tr>
                        )}
                    </tbody>
                </table>
                <div className="m-auto">
                    { this.state.page != 0 && <a href="#" onClick={this.goBackwards} >« </a> }
                    {this.state.page}
                    <a href="#" onClick={this.goForward} > »</a>
                </div>
            </div>
        }
    }
}
