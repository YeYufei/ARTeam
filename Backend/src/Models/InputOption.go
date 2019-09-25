/*
 * @Author: Yutao Ge
 * @Date: 2019-09-20 02:21:18
 * @Email: chris.dfo.only@gmail.com
 * @Last Modified by: Yutao Ge
 * @Last Modified time: 2019-09-23 23:59:54
 * @Description:
 */
package Models

import "github.com/emicklei/go-restful"

type InputType struct {
	Id       int    `json:"id" xorm:"id"`
	TypeName string `json:"typename" xorm:"typename"`
}

type TT struct {
	Id       int `json:"id" xorm:"id"`
	Qid      int
	TargetId string `json:"targetid" xorm:"targetid"`

	Questions []Question `json:"questions" xorm:"-"`
}

type Question struct {
	Id   int    `json:"id" xorm:"id"`
	Tid  int    `json:"tid" xorm:"tid"` // Input type
	Name string `json:"name" xorm:"name"`

	// Text field is used only for text area.
	Text string `json:"text" xorm:"text"`

	// Options field is only used for checkbox and radio button
	Options []string `json:"options" xorm:"options"`
}

type Answer struct {
	Id  int `json:"id" xorm:"id"`
	Uid int `json:"uid" xorm:"uid"` // User Id
	Qid int `json:"qid" xorm:"qid"` // Question Id

	Content string `json:"content" xorm:"content"`
}

type InputOptionResource struct {
}

func (i *InputOptionResource) WebService() *restful.WebService {
	ws := new(restful.WebService)
	ws.
		Path("/inputs").
		Consumes(restful.MIME_XML, restful.MIME_JSON).
		Produces(restful.MIME_JSON, restful.MIME_XML) // you can specify this per route as well

	return ws
}

func (i *InputOptionResource) GetQuestion(request *restful.Request, response *restful.Response) {

}
