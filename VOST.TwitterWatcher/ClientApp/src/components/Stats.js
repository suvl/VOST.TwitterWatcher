import React, { Component } from 'react';
import { Alert, Input } from 'reactstrap';
import { Chart } from 'react-google-charts';
import TagCloud from 'react-tag-cloud';

const randomColor = require('random-color');

export class Stats extends Component {
    constructor (props) {
        super (props);
        this.state = {
            keyword: null
        };
    }

    componentDidMount () {
        fetch ("api/v1/keywords")
            .then (res => res.json())
            .then (data => this.setState({keywords: data.map(k => k.keyword)}));
    }

    keywordSelected = (e) => {
        this.setState({keyword: e.target.value});

        fetch ("api/v1/tweets/stats?keyword="+ encodeURIComponent(e.target.value) )
            .then (res => res.json())
            .then (data => {
                const columns = [
                    {type: 'string', label: 'text'},
                    {type: 'datetime', label: 'createdAt'}
                ];

                const rows = data.histogramData
                    .map(d => [d.text, new Date(Date.parse(d.createdAt))]);
                
                const topWord = data.wordCloud[0];
                const lastWord = data.wordCloud[data.wordCloud.length - 1];
                
                const wordCloud = data.wordCloud
                    .map(k => ({
                        word: k.word, 
                        size: this.normalize (k.count, topWord.count, lastWord.count)
                    }));
                
                this.setState({ count: data.count, histogram: [columns, ...rows], words: wordCloud});
            });
    }

    normalize (val, max, min) {
        if(max - min === 0) return 50; // or b, it's up to you
        return 50 + (((val - min) * 50) / (max - min));
    }
    
    

    render () {
        return <div>
            <div>
                <Input type="select" name="keywordSelect" id="keywordSelect" onChange={this.keywordSelected}>
                    <option></option>
                    { this.state.keywords !== undefined && 
                      this.state.keywords.map(k => <option key={k}>{k}</option>) }
                </Input>
            </div>
            <div>
            { 
                this.state.keyword === null && 
                <Alert color="primary">
                    Pfv escolha uma keyword da lista
                </Alert> 
            }
            
                <div>
                    {
                        this.state.histogram !== undefined &&
                        <Chart chartType="Histogram"
                        loader={<div>Loading chart...</div>}
                        options={
                            {
                                title: 'Keyword occurrences over time',
                                legend: 'none',
                                histogram: {
                                    maxNumBuckets: 100
                                },
                                hAxis: { textPosition: 'none' },
                                height: '500px'
                            }
                        }
                        data={this.state.histogram}
                        />
                    }
                </div>

                <div>
                    {
                        this.state.words !== undefined &&
                        <TagCloud
                            style={{
                                fontFamily: 'sans-serif',
                                fontSize: 30,
                                fontWeight: 'bold',
                                fontStyle: 'italic',
                                color: () => randomColor(),
                                padding: 5,
                                width: '100%',
                                height: '500px'
                            }}>
                            {this.state.words.map(w => <div 
                                key={w.word} 
                                style={{fontSize: w.size, color: randomColor()}}>{w.word}</div>)}
                        </TagCloud>
                    }                      
                </div>
            
            </div>
        </div>
    }
}